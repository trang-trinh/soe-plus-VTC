using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace Controllers
{
    [Authorize(Roles = "login")]
    public class SQLController : ApiController
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
        #region DynamicSQL
        [HttpPost]
        public async Task<HttpResponseMessage> FilterSQLOFFSET([System.Web.Mvc.Bind(Include = "fieldSQLS,PageNo,PageSize,sqlO")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string rid = claims.Where(p => p.Type == "rid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                int OFFSET = (filterSQL.PageNo - 1) * filterSQL.PageSize;
                sql = @"
                        Select web_acess_id,user_id,full_name,is_time,is_endtime,FromIP,FromDivice from sys_web_acess
                    ";
                string WhereSQL = "";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " = N'" + m.value + "'";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  <> N'" + m.value + "'";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE " + WhereSQL;
                }
                sql += @"
                        ORDER BY " + filterSQL.sqlO + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY";
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLsys_web_acess", ip, tid, "Lỗi khi gọi FilterSQLsys_web_acess", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLsys_web_acess", ip, tid, "Lỗi khi gọi proc FilterSQLsys_web_acess", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> FilterSQLsys_web_acess([System.Web.Mvc.Bind(Include = "fieldSQLS,PageNo,PageSize,sqlO,next,id,Search")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string rid = claims.Where(p => p.Type == "rid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(web_acess_id)  as totalRecords from sys_web_acess";
            try
            {
                //int OFFSET = (filterSQL.PageNo - 1) * filterSQL.PageSize;
                sql = @"
                        Select TOP(" + filterSQL.PageSize + @") web_acess_id,user_id,full_name,is_time,is_endtime,from_ip,from_device from sys_web_acess
                    ";
                string WhereSQL = "";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"" + m.value + "*\"') ";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"*" + m.value + "\"') ";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"*" + m.value + "*\"') ";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " NOT Contains(" + field.key + ",'\"*" + m.value + "*\"') ";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }

                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        WhereSQL = " (web_acess_id" + (filterSQL.sqlO.Contains("DESC") ? "<" : ">") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        WhereSQL = " (web_acess_id" + (filterSQL.sqlO.Contains("DESC") ? ">" : "<") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "SearchName like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_Bin2";
                }
                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE " + WhereSQL;
                    sqlCount += " WHERE " + WhereSQL;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }
                if (!helper.is_admin(rid))
                {
                    string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                    sql = @" ;With CTE as
		                    (
			                    select organization_id,Capcha_ID from sys_organization where organization_id=" + dvid + @"
			                    union all
			                    Select a.organization_id,a.Capcha_ID from sys_organization a inner join cte b
			                     on b.organization_id=a.Capcha_ID 
		                    )
                            
                        " + sql + (WhereSQL.Trim() != "" ? " And " : " Where ") + @"  user_id in (Select user_id from sys_users where organization_id in (Select organization_id from CTE))";
                    //SQL Count
                    sqlCount = @" ;With CTE as
		                    (
			                    select organization_id,Capcha_ID from sys_organization where organization_id=" + dvid + @"
			                    union all
			                    Select a.organization_id,a.Capcha_ID from sys_organization a inner join cte b
			                     on b.organization_id=a.Capcha_ID 
		                    )
                            
                        " + sqlCount + (WhereSQL.Trim() != "" ? " And " : " Where ") + @"  user_id in (Select user_id from sys_users where organization_id in (Select organization_id from CTE))";

                }
                sql += @"
                        ORDER BY " + filterSQL.sqlO;
                if (filterSQL.id == null)
                {
                    sql += sqlCount;
                }
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLsys_web_acess", ip, tid, "Lỗi khi gọi FilterSQLsys_web_acess", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLsys_web_acess", ip, tid, "Lỗi khi gọi proc FilterSQLsys_web_acess", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> FilterSQLsys_logs([System.Web.Mvc.Bind(Include = "fieldSQLS,PageNo,PageSize,sqlO,next,id,Search")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(id)  as totalRecords from sys_logs";
            try
            {
                //int OFFSET = (filterSQL.PageNo - 1) * filterSQL.PageSize;
                sql = @"
                        Select TOP(" + filterSQL.PageSize + @") id,title,controller,user_id,full_name,log_date,created_ip,is_type,module,status from sys_logs
                    ";
                string WhereSQL = "";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"" + m.value + "*\"') ";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"*" + m.value + "\"') ";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"*" + m.value + "*\"') ";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " NOT Contains(" + field.key + ",'\"*" + m.value + "*\"') ";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }

                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        WhereSQL = " (id" + (filterSQL.sqlO.Contains("DESC") ? "<" : ">") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        WhereSQL = " (id" + (filterSQL.sqlO.Contains("DESC") ? ">" : "<") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "SearchName like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_Bin2";
                }
                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE " + WhereSQL;
                    sqlCount += " WHERE " + WhereSQL;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }
                sql += @"
                        ORDER BY " + filterSQL.sqlO;
                if (filterSQL.id == null)
                {
                    sql += sqlCount;
                }
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLsys_logs", ip, tid, "Lỗi khi gọi FilterSQLsys_logs", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLsys_logs", ip, tid, "Lỗi khi gọi proc FilterSQLsys_logs", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> FilterSQLtest_case([System.Web.Mvc.Bind(Include = "fieldSQLS,PageNo,PageSize,sqlO,next,id,Search")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(case_id)  as totalRecords from test_case w";
            try
            {
                //int OFFSET = (filterSQL.PageNo - 1) * filterSQL.PageSize;
                sql = @"
                        Select TOP(" + filterSQL.PageSize + @") case_id,case_name,w.status,NgayTest,IsPass,IsPassStep,SoTest,SoTestPass,full_name,u.user_id,w.created_ip from test_case w inner join sys_users u on w.created_by=u.user_id
                    ";
                string WhereSQL = "";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"" + m.value + "*\"') ";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"*" + m.value + "\"') ";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"*" + m.value + "*\"') ";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " NOT Contains(" + field.key + ",'\"*" + m.value + "*\"') ";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }

                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        WhereSQL = " (case_id" + (filterSQL.sqlO.Contains("DESC") ? "<" : ">") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        WhereSQL = " (case_id" + (filterSQL.sqlO.Contains("DESC") ? ">" : "<") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "case_name like N'%" + filterSQL.Search + "%'  or " + "Tukhoa like N'%" + filterSQL.Search + "%'";
                }
                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE " + WhereSQL;
                    sqlCount += " WHERE " + WhereSQL;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }
                sql += @"
                        ORDER BY " + filterSQL.sqlO;
                if (filterSQL.id == null)
                {
                    sql += sqlCount;
                }
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLtest_case", ip, tid, "Lỗi khi gọi FilterSQLtest_case", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLtest_case", ip, tid, "Lỗi khi gọi proc FilterSQLtest_case", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> FilterSQLsql_log([System.Web.Mvc.Bind(Include = "fieldSQLS,PageNo,PageSize,sqlO,next,id,Search")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(id)  as totalRecords from sql_log";
            try
            {
                //int OFFSET = (filterSQL.PageNo - 1) * filterSQL.PageSize;
                sql = @"
                        Select TOP(" + filterSQL.PageSize + @") id,title,controller,user_id,full_name,start_date,created_ip,milliseconds from sql_log
                    ";
                string WhereSQL = "";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"" + m.value + "*\"') ";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"*" + m.value + "\"') ";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"*" + m.value + "*\"') ";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " NOT Contains(" + field.key + ",'\"*" + m.value + "*\"') ";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }

                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        WhereSQL = " (id" + (filterSQL.sqlO.Contains("DESC") ? "<" : ">") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        WhereSQL = " (id" + (filterSQL.sqlO.Contains("DESC") ? ">" : "<") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "SearchName like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_Bin2";
                }
                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE " + WhereSQL;
                    sqlCount += " WHERE " + WhereSQL;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }
                sql += @"
                        ORDER BY " + filterSQL.sqlO;
                if (filterSQL.id == null)
                {
                    sql += sqlCount;
                }
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLsys_logs", ip, tid, "Lỗi khi gọi FilterSQLsys_logs", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLsys_logs", ip, tid, "Lỗi khi gọi proc FilterSQLsys_logs", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filtershows_main([System.Web.Mvc.Bind(Include = "fieldSQLS,PageNo,PageSize,sqlO,next,id,Search")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(shows_id)  as totalRecords from shows_main";
            try
            {
                //int OFFSET = (filterSQL.PageNo - 1) * filterSQL.PageSize;
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + "* from shows_main";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " organization_id is not null " : (" ( organization_id = 0 or organization_id =" + dvid + " ) ");

                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }


                ////Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "title like N'%" + filterSQL.Search.ToUpper() + "%' collate SQL_Latin1_General_CP1_CI_AI";
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

                if (WhereSQL.Trim() != "" || filterSQL.id != null)
                {
                    if (WhereSQL.Trim() != "")
                    {
                        WhereSQL += " and ";
                    }

                    sql += " WHERE " + WhereSQL + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz;

                    sqlCount += " WHERE " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }

                //sql += @"
                //    ORDER BY " + filterSQL.sqlO;
                if (filterSQL.id != null)
                {
                    sql += sqlCount;
                }
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filtershows_main", ip, tid, "Lỗi khi gọi Filtershows_main", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLsys_logs", ip, tid, "Lỗi khi gọi proc FilterSQLsys_logs", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }//Thêm mới ngày 17/8/2022

        [HttpPost]
        public async Task<HttpResponseMessage> Filtervideo_main([System.Web.Mvc.Bind(Include = "fieldSQLS,PageNo,PageSize,sqlO,next,id,Search")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(video_id)  as totalRecords from video_main";
            try
            {
                //int OFFSET = (filterSQL.PageNo - 1) * filterSQL.PageSize;
                sql = @"
                        Select TOP(" + filterSQL.PageSize + @") * from video_main
                    ";
                string WhereSQL = "";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }

                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        WhereSQL = " (video_id" + (filterSQL.sqlO.Contains("DESC") ? "<" : ">") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        WhereSQL = " (video_id" + (filterSQL.sqlO.Contains("DESC") ? ">" : "<") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "title like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_Bin2";
                }
                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE " + WhereSQL;
                    sqlCount += " WHERE " + WhereSQL;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }
                sql += @"
                        ORDER BY " + filterSQL.sqlO;
                if (filterSQL.id == null)
                {
                    sql += sqlCount;
                }
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filtervideo_main", ip, tid, "Lỗi khi gọi Filtervideo_main", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLsys_logs", ip, tid, "Lỗi khi gọi proc FilterSQLsys_logs", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }//Thêm mới ngày 17/8/2022

        [HttpPost]
        public async Task<HttpResponseMessage> Filternews_main([System.Web.Mvc.Bind(Include = "fieldSQLS,PageNo,PageSize,sqlO,next,id,Search")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(news_id)  as totalRecords from news_main";
            try
            {
                //int OFFSET = (filterSQL.PageNo - 1) * filterSQL.PageSize;
                //sql = @"
                //    Select TOP(" + filterSQL.PageSize + @") * from news_main
                //";
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + "* from news_main";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " organization_id is not null " : (" ( organization_id = 0 or organization_id =" + dvid + " ) ");


                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }


                ////Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "title like N'%" + filterSQL.Search.ToUpper() + "%' collate SQL_Latin1_General_CP1_CI_AI";
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

                if (WhereSQL.Trim() != "" || filterSQL.id != null)
                {
                    if (WhereSQL.Trim() != "")
                    {
                        WhereSQL += " and ";
                    }

                    sql += " WHERE " + WhereSQL + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz;

                    sqlCount += " WHERE " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }

                //sql += @"
                //    ORDER BY " + filterSQL.sqlO;
                if (filterSQL.id != null)
                {
                    sql += sqlCount;
                }
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filternews_main", ip, tid, "Lỗi khi gọi Filternews_main", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLsys_logs", ip, tid, "Lỗi khi gọi proc FilterSQLsys_logs", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }
        #endregion

        #region Từ điển
        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Stamp([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(stamp_id) as totalRecords from doc_ca_stamps ";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " stamp_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_stamps where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Stamp", ip, tid, "Lỗi khi gọi Filter_Stamp", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Stamp", ip, tid, "Lỗi khi gọi Filter_Stamp", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Doc_Ca_Postion([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(position_id) as totalRecords from doc_ca_positions ";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " position_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_positions where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Doc_CaPosition", ip, tid, "Lỗi khi gọi Filter_Doc_CaPosition", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Doc_CaPosition", ip, tid, "Lỗi khi gọi Filter_Doc_CaPosition", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Places([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string findChild = "";
                string sqlCount = @"Select count(place_id) as totalRecords from ca_places";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                { WhereSQL += " status is not null"; }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += " And is_level=" + int.Parse(filterSQL.sqlF);

                }
                else { WhereSQL += " and is_level is not null"; }
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "name like N'%" + filterSQL.Search + "%'";

                }
                findChild = "with findChild as (select parent_id from ca_places where " + WhereSQL +
                    " union all select b.parent_id from findChild as a, ca_places as b where b.place_id = a.parent_id)";
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                }

                if (WhereSQL.Trim() != "")
                {
                    sql = findChild + @" select * from ca_places where place_id in( select * from findChild) or " + WhereSQL;
                }
                else
                {
                    sql = @" select * from ca_places";
                }

                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Places", ip, tid, "Lỗi khi gọi Filter_Places", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Places", ip, tid, "Lỗi khi gọi Filter_Places", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Tag([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(tag_id) as totalRecords from doc_ca_tags ";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " tag_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_tags where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Tag", ip, tid, "Lỗi khi gọi Filter_Tag", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Tag", ip, tid, "Lỗi khi gọi Filter_Tag", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}


        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_ReceivePlace([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(receive_place_id) as totalRecords from doc_ca_receive_places";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " receive_place_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_receive_places where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_ReceivePlace", ip, tid, "Lỗi khi gọi Filter_ReceivePlace", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_ReceivePlace", ip, tid, "Lỗi khi gọi Filter_ReceivePlace", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Position([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"Select count(position_id) as totalRecords from ca_positions";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL = " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                { WhereSQL = " status is not null"; }
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? " And  " : "") + "position_name like N'%" + filterSQL.Search + "%'";

                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from ca_positions where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Position", ip, tid, "Lỗi khi gọi Filter_Position", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Position", ip, tid, "Lỗi khi gọi Filter_Position", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Type([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(doc_type_id) as totalRecords from doc_ca_types";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " doc_type_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_types where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Type", ip, tid, "Lỗi khi gọi Filter_Type", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Type", ip, tid, "Lỗi khi gọi Filter_Type", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Status([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(status_id) as totalRecords from doc_ca_status";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " status_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " is_handle = " + int.Parse(filterSQL.sqlF);
                }
                else
                { WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " is_handle is not null"; }

                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_status where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Status", ip, tid, "Lỗi khi gọi Filter_Status", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Status", ip, tid, "Lỗi khi gọi Filter_Status", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_CaGroup([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(doc_group_id) as totalRecords from doc_ca_groups";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " doc_group_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_groups where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_CaGroup", ip, tid, "Lỗi khi gọi Filter_CaGroup", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_CaGroup", ip, tid, "Lỗi khi gọi Filter_CaGroup", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Signer([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(signer_id) as totalRecords from doc_ca_signers";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " signer_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (" (organization_id= " + int.Parse(dvid) + ")"));
                }
                else
                { WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " organization_id is not null"; }

                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_signers where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Signer", ip, tid, "Lỗi khi gọi Filter_Signer", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Signer", ip, tid, "Lỗi khi gọi Filter_Signer", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Sendway([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(send_way_id) as totalRecords from doc_ca_send_ways";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " send_way_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_send_ways where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Sendway", ip, tid, "Lỗi khi gọi Filter_Sendway", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Sendway", ip, tid, "Lỗi khi gọi Filter_Sendway", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Security([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(security_id) as totalRecords from doc_ca_security";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " security_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_security where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Security", ip, tid, "Lỗi khi gọi Filter_Security", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Security", ip, tid, "Lỗi khi gọi Filter_Security", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Urgency([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(urgency_id) as totalRecords from doc_ca_urgency";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " urgency_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_urgency where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Urgency", ip, tid, "Lỗi khi gọi Filter_Urgency", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Urgency", ip, tid, "Lỗi khi gọi Filter_Urgency", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_IssuePlace([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(issue_place_id) as totalRecords from doc_ca_issue_places";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " issue_place_name like N'%" + filterSQL.Search + "%' or search_code like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select *,(select max(z.is_order) from doc_ca_issue_places z where z.parent_id=p.issue_place_id And z.organization_id=" + (uid == "administrator" ? 0 : int.Parse(dvid)) + ") AS maxIsOrder from doc_ca_issue_places p where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @" ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_IssuePlace", ip, tid, "Lỗi khi gọi Filter_IssuePlace", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_IssuePlace", ip, tid, "Lỗi khi gọi Filter_IssuePlace", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }
        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Field([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(field_id) as totalRecords from doc_ca_fields";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " field_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_fields where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Field", ip, tid, "Lỗi khi gọi Filter_Field", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Field", ip, tid, "Lỗi khi gọi Filter_Field", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_EmailGroup([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(email_group_id) as totalRecords from doc_ca_email_groups";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " email_group_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select *,(SELECT count(dce.email_group_id) FROM doc_ca_emails AS dce WHERE dce.email_group_id=dceg.email_group_id) email_count from doc_ca_email_groups dceg where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_EmailGroup", ip, tid, "Lỗi khi gọi Filter_EmailGroup", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_EmailGroup", ip, tid, "Lỗi khi gọi Filter_EmailGroup", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Email([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlO,PageNo,PageSize,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(email_id) as totalRecords from doc_ca_emails";
                string WhereSQL = "";
                WhereSQL += " email_group_id= " + int.Parse(filterSQL.id);
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (" And  ") : "") + " email_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                //if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                //{
                //    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + (" organization_id = " + int.Parse(filterSQL.sqlF));
                //}
                //else
                //{ WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") + (" organization_id = 0 or organization_id = " + int.Parse(dvid)); }

                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_emails where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"   
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Email", ip, tid, "Lỗi khi gọi Filter_Email", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Email", ip, tid, "Lỗi khi gọi Filter_Email", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Dispatch([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(dispatch_book_id) as totalRecords from doc_ca_dispatch_books";
                string WhereSQL = "";

                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " dispatch_book_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }

                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (" (organization_id= " + int.Parse(dvid) + ")"));
                }
                else
                { WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " organization_id is not null"; }

                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select * from doc_ca_dispatch_books where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += @"
                        ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc") + @"
                        OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Dispatch_books", ip, tid, "Lỗi khi gọi Filter_Dispatch_books", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Dispatch_books", ip, tid, "Lỗi khi gọi Filter_Dispatch_books", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }
        #endregion

        #region Law        
        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Law_Doc_Language([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(law_language_id) as totalRecords from law_doc_language ";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? (WhereSQL + "and  ") : "") + " language_name like N'%" + filterSQL.Search + "%'";

                }

                WhereSQL = WhereSQL.Trim();
                if (WhereSQL.StartsWith("and "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                else if (WhereSQL.StartsWith("or "))
                {
                    WhereSQL = WhereSQL.Substring(2);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " status is not null";
                }

                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + (" organization_id = " + int.Parse(filterSQL.sqlF));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + (" organization_id = 0 or organization_id = " + int.Parse(dvid)));
                }

                if (WhereSQL.Trim() != "")
                {
                    sqlCount += Environment.NewLine + "where " + WhereSQL;
                    sql = @"select * from law_doc_language"
                        + Environment.NewLine + "where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += Environment.NewLine + @"ORDER BY " + (filterSQL.sqlO == null || (filterSQL.sqlO.Length > 60 || filterSQL.sqlO.Contains("select") || filterSQL.sqlO.Contains("update") || filterSQL.sqlO.Contains("delete") || filterSQL.sqlO.Contains("drop")) ? "created_date DESC, is_order DESC" : filterSQL.sqlO)
                     + Environment.NewLine + @"OFFSET " + OFFSET
                     + Environment.NewLine + @"ROWS FETCH NEXT " + filterSQL.PageSize
                     + Environment.NewLine + @"ROWS ONLY ";
                sql += Environment.NewLine + sqlCount;
                //sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                //sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Law_Doc_Language", ip, tid, "Lỗi khi gọi Filter_Law_Doc_Language", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Law_Doc_Language", ip, tid, "Lỗi khi gọi Filter_Law_Doc_Language", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Law_Doc_Types([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(law_type_id) as totalRecords from law_doc_types ";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? (WhereSQL + "and  ") : "") + " law_type_name like N'%" + filterSQL.Search + "%'";

                }
                WhereSQL = WhereSQL.Trim();
                if (WhereSQL.StartsWith("and "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                else if (WhereSQL.StartsWith("or "))
                {
                    WhereSQL = WhereSQL.Substring(2);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " status is not null";
                }

                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + (" organization_id = " + int.Parse(filterSQL.sqlF));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + (" organization_id = 0 or organization_id = " + int.Parse(dvid)));
                }

                if (WhereSQL.Trim() != "")
                {
                    sqlCount += Environment.NewLine + "where " + WhereSQL;
                    sql = @"select * from law_doc_types"
                        + Environment.NewLine + "where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += Environment.NewLine + @"ORDER BY " + (filterSQL.sqlO == null || (filterSQL.sqlO.Length > 60 || filterSQL.sqlO.Contains("select") || filterSQL.sqlO.Contains("update") || filterSQL.sqlO.Contains("delete") || filterSQL.sqlO.Contains("drop")) ? "created_date DESC, is_order DESC" : filterSQL.sqlO)
                     + Environment.NewLine + @"OFFSET " + OFFSET
                     + Environment.NewLine + @"ROWS FETCH NEXT " + filterSQL.PageSize
                     + Environment.NewLine + @"ROWS ONLY ";
                sql += Environment.NewLine + sqlCount;
                //sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                //sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Law_Doc_Types", ip, tid, "Lỗi khi gọi Filter_Law_Doc_Types", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Law_Doc_Types", ip, tid, "Lỗi khi gọi Filter_Law_Doc_Types", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Law_Doc_Issue_Places([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(issue_place_id) as totalRecords from law_doc_issue_places ";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? (WhereSQL + "and  ") : "") + " issue_place_name like N'%" + filterSQL.Search + "%'";

                }
                WhereSQL = WhereSQL.Trim();
                if (WhereSQL.StartsWith("and "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                else if (WhereSQL.StartsWith("or "))
                {
                    WhereSQL = WhereSQL.Substring(2);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " status is not null";
                }

                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + (" organization_id = " + int.Parse(filterSQL.sqlF));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + (" organization_id = 0 or organization_id = " + int.Parse(dvid)));
                }

                if (WhereSQL.Trim() != "")
                {
                    sqlCount += Environment.NewLine + "where " + WhereSQL;
                    sql = @" select * from law_doc_issue_places"
                        + Environment.NewLine + "where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += Environment.NewLine + @"ORDER BY " + (filterSQL.sqlO == null || (filterSQL.sqlO.Length > 60 || filterSQL.sqlO.Contains("select") || filterSQL.sqlO.Contains("update") || filterSQL.sqlO.Contains("delete") || filterSQL.sqlO.Contains("drop")) ? "created_date DESC, is_order DESC" : filterSQL.sqlO)
                     + Environment.NewLine + @"OFFSET " + OFFSET
                     + Environment.NewLine + @"ROWS FETCH NEXT " + filterSQL.PageSize
                     + Environment.NewLine + @"ROWS ONLY ";
                sql += Environment.NewLine + sqlCount;
                //sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                //sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Law_Doc_Issue_Places", ip, tid, "Lỗi khi gọi Filter_Law_Doc_Issue_Places", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Law_Doc_Issue_Places", ip, tid, "Lỗi khi gọi Filter_Law_Doc_Issue_Places", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Law_Doc_Fields([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(law_field_id) as totalRecords from law_doc_fields ";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? (WhereSQL + "and  ") : "") + " field_name like N'%" + filterSQL.Search + "%'";

                }
                WhereSQL = WhereSQL.Trim();
                if (WhereSQL.StartsWith("and "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                else if (WhereSQL.StartsWith("or "))
                {
                    WhereSQL = WhereSQL.Substring(2);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + (" organization_id = " + int.Parse(filterSQL.sqlF));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + (" organization_id = 0 or organization_id = " + int.Parse(dvid)));
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += Environment.NewLine + "where " + WhereSQL;
                    sql = @"select * from law_doc_fields"
                        + Environment.NewLine + "where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += Environment.NewLine + @"ORDER BY " + (filterSQL.sqlO == null || (filterSQL.sqlO.Length > 60 || filterSQL.sqlO.Contains("select") || filterSQL.sqlO.Contains("update") || filterSQL.sqlO.Contains("delete") || filterSQL.sqlO.Contains("drop")) ? "created_date DESC, is_order DESC" : filterSQL.sqlO)
                     + Environment.NewLine + @"OFFSET " + OFFSET
                     + Environment.NewLine + @"ROWS FETCH NEXT " + filterSQL.PageSize
                     + Environment.NewLine + @"ROWS ONLY ";
                sql += Environment.NewLine + sqlCount;
                //sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                //sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Law_Doc_Fields", ip, tid, "Lỗi khi gọi Filter_Law_Doc_Fields", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Law_Doc_Fields", ip, tid, "Lỗi khi gọi Filter_Law_Doc_Fields", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_Law_Doc_Signers([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO,PageNo,PageSize")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(signer_id) as totalRecords from law_doc_signers ";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? (WhereSQL + "and  ") : "") + " signer_name like N'%" + filterSQL.Search + "%'";

                }
                WhereSQL = WhereSQL.Trim();
                if (WhereSQL.StartsWith("and "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                else if (WhereSQL.StartsWith("or "))
                {
                    WhereSQL = WhereSQL.Substring(2);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + " status is not null";
                }

                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + (" organization_id = " + int.Parse(filterSQL.sqlF));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (Environment.NewLine + (WhereSQL.Trim() != "" ? "and " : "") + (" organization_id = 0 or organization_id = " + int.Parse(dvid)));
                }

                if (WhereSQL.Trim() != "")
                {
                    sqlCount += Environment.NewLine + "where " + WhereSQL;
                    sql = @"select * from law_doc_signers"
                        + Environment.NewLine + "where " + WhereSQL;
                }
                string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                sql += Environment.NewLine + @"ORDER BY " + (filterSQL.sqlO == null || (filterSQL.sqlO.Length > 60 || filterSQL.sqlO.Contains("select") || filterSQL.sqlO.Contains("update") || filterSQL.sqlO.Contains("delete") || filterSQL.sqlO.Contains("drop")) ? "created_date DESC, is_order DESC" : filterSQL.sqlO)
                     + Environment.NewLine + @"OFFSET " + OFFSET
                     + Environment.NewLine + @"ROWS FETCH NEXT " + filterSQL.PageSize
                     + Environment.NewLine + @"ROWS ONLY ";
                sql += Environment.NewLine + sqlCount;
                //sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                //sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_Law_Doc_Signers", ip, tid, "Lỗi khi gọi Filter_Law_Doc_Signers", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_Law_Doc_Signers", ip, tid, "Lỗi khi gọi Filter_Law_Doc_Signers", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }
        #endregion

        #region Tài sản



        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_config_number([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(config_number_id)  as totalRecords from device_config_number";
            try
            {
                //int OFFSET = (filterSQL.PageNo - 1) * filterSQL.PageSize;

                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                //string WhereSQL = super == "True" ? "" : (" ( organization_id = 0 or organization_id =" + dvid + " ) ");
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql += selectStr + " * from device_config_number ";


                string WhereSQL = "";
                string checkOrgz = super == "True" ? " organization_id is not null " : (" ( organization_id = 0 or organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
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
                    if (filterSQL.id != "-1" && filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "current_number like N'%" + filterSQL.Search.ToUpper() + "%' or text_symbols like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_CI_AS";
                }



                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_warehouse", ip, tid, "Lỗi khi gọi Filter_warehouse", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_warehouse", ip, tid, "Lỗi khi gọi proc Filter_warehouse", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Filter_warehouse([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(warehouse_id)  as totalRecords from device_warehouse";
            try
            {

                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql += selectStr + " * from device_warehouse  ";
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " organization_id is not null " : (" ( organization_id = 0 or organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
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
                    if (filterSQL.id != "-1" && filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "warehouse_name like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_CI_AS";
                }



                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_warehouse", ip, tid, "Lỗi khi gọi Filter_warehouse", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_warehouse", ip, tid, "Lỗi khi gọi proc Filter_warehouse", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_unit([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(device_unit_id)  as totalRecords from device_unit";
            try
            {
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql += selectStr + " * from device_unit  ";
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " organization_id is not null " : (" ( organization_id = 0 or organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
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
                    if (filterSQL.id != "-1" && filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "device_unit_name like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_CI_AS";
                }



                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_unit", ip, tid, "Lỗi khi gọi Filter_device_unit", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_unit", ip, tid, "Lỗi khi gọi proc Filter_device_unit", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_type([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(device_type_id)  as totalRecords from device_type";
            try
            {
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql += selectStr + " * from device_type  ";
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " organization_id is not null " : (" ( organization_id = 0 or organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
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
                    if (filterSQL.id != "-1" && filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "device_type_name like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_CI_AS";
                }



                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_type", ip, tid, "Lỗi khi gọi Filter_device_type", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_type", ip, tid, "Lỗi khi gọi proc Filter_device_type", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_groups([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(device_groups_id)  as totalRecords from device_groups cp";
            try
            {
                //int OFFSET = (filterSQL.PageNo - 1) * filterSQL.PageSize;


                sql = @"
                       
        WITH temp(device_groups_id
              , groups_name
              , groups_code
              , parent_id
              , organization_id
              , status
              , is_order
              , created_by)
        as (
              ( Select TOP(" + filterSQL.PageSize + @") * from (select  cp.device_groups_id
      ,cp.groups_name
      ,cp.groups_code
      ,cp.parent_id
      ,cp.organization_id
      ,cp.status
      ,cp.is_order
      ,cp.created_by
                From device_groups cp
) as cp
";
                var sql2 = " ) Union All (Select cp.device_groups_id, cp.groups_name , cp.groups_code , cp.parent_id, cp.organization_id , cp.status, cp.is_order, cp.created_by From device_groups as cp, temp as b " +
                                        "Where b.device_groups_id = cp.parent_id ))";
                //sql = @"
                //    Select TOP(" + filterSQL.PageSize + @") * from device_groups
                //";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = super == "True" ? "" : (" ( organization_id = 0 or organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }

                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        WhereSQL = "offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only" + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1" && filterSQL.id != null)
                    {
                        WhereSQL = "offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only" + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " cp.groups_name like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_CI_AS";
                }
                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE " + WhereSQL + " and cp.parent_id is null";
                    sqlCount += " WHERE " + WhereSQL + " and cp.parent_id is null";
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }
                sql += @"
                        ORDER BY " + filterSQL.sqlO;
                if (filterSQL.id == null)
                {
                    sql += sql2 + " select * from temp " + sqlCount;
                }
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_groups", ip, tid, "Lỗi khi gọi Filter_device_groups", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_groups", ip, tid, "Lỗi khi gọi proc Filter_device_groups", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_provider([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(provider_id)  as totalRecords from device_provider";
            try
            {
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql += selectStr + " * from device_provider  ";
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " organization_id is not null " : (" ( organization_id = 0 or organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
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
                    if (filterSQL.id != "-1" && filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "provider_name like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_CI_AS";
                }



                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE " + checkOrgz;
                }
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_provider", ip, tid, "Lỗi khi gọi Filter_device_provider", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_provider", ip, tid, "Lỗi khi gọi proc Filter_device_provider", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_main([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(device_id)  as totalRecords from device_main cp";
            try
            {
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql += selectStr + " * from device_main cp  ";
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " cp.organization_id is not null " : (" ( cp.organization_id = 0 or cp.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (  cp." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
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
                    if (filterSQL.id != "-1" && filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "device_name like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_CI_AS";
                }



                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_main", ip, tid, "Lỗi khi gọi Filter_device_main", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_main", ip, tid, "Lỗi khi gọi proc Filter_device_main", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_card([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            //var param = new DynamicParameters();
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(card_id)  as totalRecords from device_card tm";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + " tm.*,( SELECT dw.warehouse_name FROM device_warehouse dw WHERE dw.warehouse_id = tm.warehouse_id) AS warehouse_name " +
                    ",  (SELECT ds.device_status_name FROM device_status ds WHERE ds.device_status_code = tm.status) AS device_status_name," +
                    "su.full_name AS device_user_name, so.organization_name AS manage_department_name from device_card tm LEFT JOIN sys_users su ON tm.device_user_id=su.user_id LEFT JOIN sys_organization so ON tm.manage_department_id =so.organization_id ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";

                string checkOrgz = super == "True" ? " tm.organization_id is not null " : (" ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) ");
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
                                        WhereSQLR += " " + field.filteroperator + " ( tm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( tm." + field.key + "  <> N'" + m.value + "')";
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

                        + " CONTAINS( tm.device_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') " +

                        " or ( tm.barcode_id like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +
                            " or ( tm.device_number like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi Filter_device_card", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi proc Filter_device_card", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_report_expiration([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(card_id)  as totalRecords from device_card tm";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + "tm.*,( SELECT dw.warehouse_name FROM device_warehouse dw WHERE dw.warehouse_id = tm.warehouse_id) AS warehouse_name ," +
                    "su.full_name AS device_user_name, so.organization_name AS manage_department_name," +
                    " (SELECT so1.organization_name FROM sys_organization so1 WHERE so1.organization_id = tm.corporation) AS corporation_name," +
                    "(SELECT ds.full_name FROM sys_users ds WHERE ds.user_id = tm.created_by) AS created_name," +
                    "(SELECT ds.device_status_name FROM device_status ds WHERE ds.device_status_code = tm.status) AS device_status_name" +
                    " from device_card tm LEFT JOIN sys_users su ON tm.device_user_id=su.user_id LEFT JOIN sys_organization so ON tm.manage_department_id =so.organization_id ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = " (tm.price IS NULL OR tm.depreciation_month IS NULL OR tm.depreciation_month = 0" +
                    " OR(tm.depreciation_month > 0 AND((DATEDIFF(MONTH, tm.purchase_date, GETDATE()) - tm.depreciation_month) > 0" +
                    " AND DATEDIFF(MONTH, tm.purchase_date, GETDATE()) > 0 )" +
                    "  OR((DAY(GETDATE()) - DAY(tm.purchase_date)) > 0" +
                    " AND(DATEDIFF(MONTH, tm.purchase_date, GETDATE()) - tm.depreciation_month) = 0)) )";
                string checkOrgz = super == "True" ? " tm.organization_id is not null " : (" ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQL += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQL += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                    }
                }

                if (WhereSQL.StartsWith("( and"))
                {

                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                        + "(( tm.device_name like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )" +
                        " or ( tm.barcode_id like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +
                            " or ( tm.device_number like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +
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
                    sql += " WHERE " + WhereSQL + " and " + checkOrgz + @"
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi Filter_device_card", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi proc Filter_device_card", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_report_insurance([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id,Date")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(card_id)  as totalRecords from device_card tm";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + "tm.*,( SELECT dw.warehouse_name FROM device_warehouse dw WHERE dw.warehouse_id = tm.warehouse_id) AS warehouse_name ," +
                    "su.full_name AS device_user_name, so.organization_name AS manage_department_name," +
                    " (SELECT so1.organization_name FROM sys_organization so1 WHERE so1.organization_id = tm.corporation) AS corporation_name," +
                    "(SELECT ds.full_name FROM sys_users ds WHERE ds.user_id = tm.created_by) AS created_name," +
                    "(SELECT ds.device_status_name FROM device_status ds WHERE ds.device_status_code = tm.status) AS device_status_name" +
                    " from device_card tm LEFT JOIN sys_users su ON tm.device_user_id=su.user_id LEFT JOIN sys_organization so ON tm.manage_department_id =so.organization_id ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = " dbo.checkInsurance(tm.purchase_date,tm.insurance_cycle,tm.insurance_month ,'" + filterSQL.Date + "' ) =1";
                string checkOrgz = super == "True" ? " tm.organization_id is not null " : (" ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQL += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQL += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                    }
                }

                if (WhereSQL.StartsWith("( and"))
                {

                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                        + "(( tm.device_name like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )" +
                        " or ( tm.barcode_id like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +
                            " or ( tm.device_number like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +
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
                    sql += " WHERE " + WhereSQL + " and " + checkOrgz + @"
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi Filter_device_card", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi proc Filter_device_card", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_report_year([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(card_id)  as totalRecords from device_card tm";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + "tm.*,( SELECT dw.warehouse_name FROM device_warehouse dw WHERE dw.warehouse_id = tm.warehouse_id) AS warehouse_name ,su.full_name AS device_user_name, so.organization_name AS manage_department_name from device_card tm LEFT JOIN sys_users su ON tm.device_user_id=su.user_id LEFT JOIN sys_organization so ON tm.manage_department_id =so.organization_id ";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " tm.organization_id is not null " : (" ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQL += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQL += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                    }
                }

                if (WhereSQL.StartsWith("( and"))
                {

                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                        + "(( tm.device_name like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )" +
                        " or ( tm.barcode_id like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +
                            " or ( tm.device_number like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +
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
                    sql += " WHERE " + WhereSQL + " and " + checkOrgz + @"
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi Filter_device_card", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi proc Filter_device_card", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_manufacturer([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;

            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(device_manufacturer_id)  as totalRecords from device_manufacturer";
            try
            {
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql += selectStr + " * from device_manufacturer  ";
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " organization_id is not null " : (" ( organization_id = 0 or organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( " + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
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
                    if (filterSQL.id != "-1" && filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "device_manufacturer_name like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_CI_AS";
                }



                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi Filter_device_card", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi proc Filter_device_card", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_status([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(device_status_id)  as totalRecords from device_status";
            try
            {
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql += selectStr + " * from device_status   ";
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " organization_id is not null " : (" ( organization_id = 0 or organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( " + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
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
                    if (filterSQL.id != "-1" && filterSQL.id != null)
                    {
                        offSetSQL = " offset (" + filterSQL.PageNo * filterSQL.PageSize + ") rows fetch next " + filterSQL.PageSize + " rows only";
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")

                    + "( ( device_status_name like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )" +
                     " or ( device_status_code like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +

                     ")";
                }



                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_status", ip, tid, "Lỗi khi gọi Filter_device_status", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_status", ip, tid, "Lỗi khi gọi proc Filter_device_status", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_handover([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(handover_id)  as totalRecords from device_handover tm";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + "tm.*,( SELECT COUNT(*) FROM device_handover_attach dha WHERE dha.handover_id = tm.handover_id) AS assets from device_handover tm";
                string ad = claims.Where(x => x.Type == "ad").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = ad == "True" ? " ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " : (" ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " +
                    " AND ( tm.created_by= '" + uid + "' OR tm.user_deliver_id= '" + uid + "' ) ");
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                if (super == "True")
                    checkOrgz = " tm.organization_id is not null ";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQL += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQL += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                    }
                }

                if (WhereSQL.StartsWith("( and"))
                {

                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "( tm.handover_number like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )"

                        + " or CONTAINS( tm.user_receiver_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') " +
                         " or CONTAINS( tm.user_deliver_name,'\"*" + filterSQL.Search.ToUpper() + "*\"') ";
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
                    sql += " WHERE ( " + WhereSQL + " )  and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO;

                    sqlCount += " WHERE " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }


                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_handover", ip, tid, "Lỗi khi gọi Filter_device_handover", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_handover", ip, tid, "Lỗi khi gọi proc Filter_device_handover", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_accept_device_handover([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(handover_id)  as totalRecords from device_handover tm";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + "tm.*,( SELECT COUNT(*) FROM device_handover_attach dha WHERE dha.handover_id = tm.handover_id) AS assets from device_handover tm";
                string ad = claims.Where(x => x.Type == "ad").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = ad == "True" ? " ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " : (" ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " +
                    " AND ( tm.created_by= '" + uid + "' OR tm.user_deliver_id= '" + uid + "'  OR tm.user_verifier_id= '" + uid + "'  OR tm.user_receiver_id = '" + uid + "'  ) ");
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                if (super == "True")
                    checkOrgz = " tm.organization_id is not null ";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQL += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQL += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                    }
                }

                if (WhereSQL.StartsWith("( and"))
                {

                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                var Searh = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    Searh = " and ( ( tm.handover_number like N'%" + filterSQL.Search.ToUpper()
                        + "%'  collate Latin1_General_100_CI_AS )" +
                        " or ( tm.user_receiver_name like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )"
                        + " or ( tm.user_verifier_name like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )"
                        + " or ( tm.user_deliver_name like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  ) ";
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
                    sql += " WHERE   (" + WhereSQL + ")   " + Searh + "    and (status <> N'0') and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " )   " + Searh + " and (status <> N'0') and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE    (status <> N'0')  " + Searh + "    and" + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO;

                    sqlCount += " WHERE   (status <> N'0')  " + Searh + "  and " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }


                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_handover", ip, tid, "Lỗi khi gọi Filter_device_handover", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_handover", ip, tid, "Lỗi khi gọi proc Filter_device_handover", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_repair([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(device_repair_id)  as totalRecords from device_repair tm";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + "tm.*,( SELECT COUNT(*) FROM device_repair_details dha WHERE dha.device_repair_id = tm.device_repair_id) AS countDevice" +
                    ",CASE WHEN tm.warehouse_id IS NOT NULL THEN(SELECT dw.warehouse_name FROM device_warehouse dw WHERE dw.warehouse_id = tm.warehouse_id)" +
                    " WHEN tm.department_id IS NOT NULL THEN(SELECT so.organization_name FROM sys_organization so  WHERE so.organization_id = tm.department_id) " +
                    "END AS deviceFrom from device_repair tm";
                string ad = claims.Where(x => x.Type == "ad").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = ad == "True" ? " ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " : (" ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " +
                    " AND ( tm.created_by= '" + uid + "' OR tm.repair_user_id= '" + uid + "'   ) ");
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                if (super == "True")
                    checkOrgz = " tm.organization_id is not null ";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQL += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQL += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                    }
                }

                if (WhereSQL.StartsWith("( and"))
                {

                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                         + "(( tm.repair_number like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )" +

                         " or CONTAINS( tm.proposer,'\"*" + filterSQL.Search.ToUpper() + "*\"') " +

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
                    sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
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
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_repair", ip, tid, "Lỗi khi gọi Filter_device_repair", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_repair", ip, tid, "Lỗi khi gọi proc Filter_device_repair", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_inventory_slip([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(inventory_slip_id)  as totalRecords from device_inventory_slip  tm";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + "tm.*,( SELECT count(*) FROM device_inventory_personnel  drd WHERE drd.inventory_slip_id = tm.inventory_slip_id AND drd.is_approved=0) AS countDevice" +
                    ",  su.full_name,su.avatar  from device_inventory_slip tm left JOIN sys_users su ON su.user_id = tm.inventory_user_id  ";
                string ad = claims.Where(x => x.Type == "ad").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = ad == "True" ? " ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " : (" ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " +
                    " AND ( tm.created_by= '" + uid + "' OR tm.inventory_user_id= '" + uid + "'   ) ");
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                if (super == "True")
                    checkOrgz = " tm.organization_id is not null ";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var andF = "";
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        var srcWhere = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {

                            srcWhere += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        srcWhere += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        srcWhere += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        srcWhere += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        srcWhere += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        srcWhere += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        srcWhere += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        srcWhere += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        srcWhere += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        srcWhere += " " + field.filteroperator + " ( tm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        srcWhere += " " + field.filteroperator + " ( tm." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        srcWhere += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        srcWhere += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        srcWhere += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        srcWhere += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }

                            srcWhere += field.filterconstraints.Count > 1 ? ")" : "";

                        }
                        if (srcWhere.StartsWith("( and"))
                        {

                            srcWhere = "( " + WhereSQL.Substring(5);
                        }
                        else if (srcWhere.StartsWith("( or"))
                        {
                            srcWhere = "( " + srcWhere.Substring(4);
                        }
                        if (srcWhere.StartsWith(" and"))
                        {

                            srcWhere = srcWhere.Substring(4);
                        }
                        else if (srcWhere.StartsWith(" or"))
                        {
                            srcWhere = srcWhere.Substring(3);
                        }
                        WhereSQL = WhereSQL + andF + srcWhere;
                        andF = " and ";
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") +
                        "( tm.inventory_number like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )";
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
                    sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL; ;

                    sqlCount += " WHERE " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }


                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_inventory_slip", ip, tid, "Lỗi khi gọi Filter_device_inventory_slip", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_inventory_slip", ip, tid, "Lỗi khi gọi proc Filter_device_inventory_slip", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_accept_inventory_slip([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(inventory_slip_id)  as totalRecords from device_inventory_slip  tm "
                + "INNER JOIN sys_users su ON su.user_id = tm.inventory_user_id";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + "tm.*,( SELECT count(*) FROM device_inventory_personnel  drd WHERE drd.inventory_slip_id = tm.inventory_slip_id AND drd.is_approved=0) AS countDevice" +
                    ", su.full_name ,su.avatar  from device_inventory_slip tm " +
                    "INNER JOIN sys_users su ON su.user_id = tm.inventory_user_id";
                string ad = claims.Where(x => x.Type == "ad").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = ad == "True" ? " ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " : (" ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " +
                    "  and tm.status!=0 AND (SELECT COUNT(*) FROM device_inventory_personnel dip  WHERE dip.inventory_slip_id = tm.inventory_slip_id      AND dip.user_id = '" + uid + "' ) >0 ");
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                if (super == "True")
                    checkOrgz = " tm.organization_id is not null ";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var andF = "";
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        var srcWhere = "";
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {

                            srcWhere += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        srcWhere += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        srcWhere += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        srcWhere += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        srcWhere += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        srcWhere += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        srcWhere += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        srcWhere += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        srcWhere += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        srcWhere += " " + field.filteroperator + " ( tm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        srcWhere += " " + field.filteroperator + " ( tm." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        srcWhere += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        srcWhere += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        srcWhere += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        srcWhere += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }

                            srcWhere += field.filterconstraints.Count > 1 ? ")" : "";

                        }
                        if (srcWhere.StartsWith("( and"))
                        {

                            srcWhere = "( " + WhereSQL.Substring(5);
                        }
                        else if (srcWhere.StartsWith("( or"))
                        {
                            srcWhere = "( " + srcWhere.Substring(4);
                        }
                        if (srcWhere.StartsWith(" and"))
                        {

                            srcWhere = srcWhere.Substring(4);
                        }
                        else if (srcWhere.StartsWith(" or"))
                        {
                            srcWhere = srcWhere.Substring(3);
                        }
                        WhereSQL = WhereSQL + andF + srcWhere;
                        andF = " and ";
                    }
                }


                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                         + "(( tm.inventory_number like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )" +
                         " or ( su.full_name like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +

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
                    sql += " WHERE ( " + WhereSQL + " )  and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE tm.status!=0 and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL; ;

                    sqlCount += " WHERE tm.status!=0 and " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }


                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_inventory_slip", ip, tid, "Lỗi khi gọi Filter_device_inventory_slip", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_inventory_slip", ip, tid, "Lỗi khi gọi proc Filter_device_inventory_slip", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }


        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_recall([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(device_recall_id)  as totalRecords from device_recall  tm";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + " tm.*,(SELECT count(*) FROM device_recall_details drd WHERE drd.device_recall_id = tm.device_recall_id) AS countDevice," +
                    " su.full_name,su.avatar, su1.full_name AS product_name,su1.avatar AS product_avatar from device_recall tm" +
                    "   left JOIN sys_users su ON su.user_id = tm.user_collector LEFT JOIN sys_users su1  ON su1.user_id = tm.product_user";
                string ad = claims.Where(x => x.Type == "ad").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = ad == "True" ? " ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " : (" ( tm.organization_id = 0 or tm.organization_id =" + dvid + " ) " +
                    " AND (    tm.user_collector= '" + uid + "' OR tm.product_user= '" + uid + "'  OR tm.stocker= '" + uid + "'   ) ");
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                if (super == "True")
                    checkOrgz = " tm.organization_id is not null ";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQL += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " ( tm." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQL += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                    }
                }

                if (WhereSQL.StartsWith("( and"))
                {

                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") +
                        "( tm.recall_number like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )";
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
                    sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL; ;

                    sqlCount += " WHERE " + checkOrgz;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }


                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_recall", ip, tid, "Lỗi khi gọi Filter_device_recall", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_recall", ip, tid, "Lỗi khi gọi proc Filter_device_recall", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }



        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_approved_group([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(cp.approved_group_id)  as totalRecords from device_approved_group cp";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + " cp.*," +
                    " ( CASE When cp.is_approved_by_department = 1 THEN (SELECT COUNT(*) FROM device_approved_department_group dadg WHERE dadg.approved_group_id = cp.approved_group_id) " +
                    "WHEN cp.is_approved_by_department = 0 THEN (SELECT COUNT(*) FROM device_approved_department_user dudg WHERE dudg.approved_group_id = cp.approved_group_id) END" +
                    ") AS users_count from device_approved_group cp";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " cp.organization_id is not null " : (" ( cp.organization_id = 0 or cp.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQL += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (cp." + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (cp." + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (cp." + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (cp." + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " cp." + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " cp." + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " cp." + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " cp." + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( cp." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " ( cp." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST( cp." + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST( cp." + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST( cp." + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST( cp." + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQL += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                    }
                }

                if (WhereSQL.StartsWith("( and"))
                {

                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "( cp.approved_group_name like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )";
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

                if (WhereSQL.Trim() != "" || filterSQL.id != null)
                {
                    if (WhereSQL.Trim() != "")
                    {
                        WhereSQL += " and ";
                    }
                    sql += " WHERE " + WhereSQL + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + checkOrgz;
                    //sql += " WHERE ( " + WhereSQL + " ) and " + checkOrgz + @"
                    //ORDER BY " + filterSQL.sqlO + offSetSQL;
                    //sqlCount += " WHERE ( " + WhereSQL + " ) and " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO;

                    sqlCount += " WHERE " + checkOrgz;
                }


                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }


                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_repair", ip, tid, "Lỗi khi gọi Filter_device_repair", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_repair", ip, tid, "Lỗi khi gọi proc Filter_device_repair", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_device_follows_all([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";

            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";
                sql = selectStr + "    dp.*,"
+ " (SELECT su.full_name FROM sys_users su WHERE su.user_id = dp.created_by) as created_name,"
+ " (SELECT su.avatar FROM sys_users su WHERE su.user_id = dp.created_by) AS created_avatar INTO #bangTamDL1 "
+ " FROM device_process dp";

                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " dp.organization_id is not null " : (" ( dp.organization_id = 0 or dp.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQL += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "isNull":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " is null  )";
                                        break;
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " dp." + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " dp." + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " dp." + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " dp." + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( dp." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " ( dp." + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(dp." + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(dp." + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(dp." + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(dp." + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQL += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                    }
                }

                if (WhereSQL.StartsWith("( and"))
                {

                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                        + "(( dp.device_process_code like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )" +
                        " or ( dp.content like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +
                        //" or ( dp.device_number like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS )  " +
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
                    sql += " WHERE " + WhereSQL + " and " +
                        " ( dp.device_process_code in  ( SELECT de.device_process_code from dbo.udfGetProcess(  '" + uid + "' ) de )     and " +
                         " dp.device_note_id in  ( SELECT de.device_note_id from dbo.udfGetProcess(  '" + uid + "' ) de ) AND dp.is_view = 1  ) AND (dp.organization_id = 0 OR dp.organization_id = '" + dvid + "')";
                    sql += " SELECT * FROM #bangTamDL1 dp where " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                }
                else
                {
                    sql += " WHERE " +
                       " ( dp.device_process_code in  ( SELECT de.device_process_code from dbo.udfGetProcess(  '" + uid + "' ) de )     and " +
                         " dp.device_note_id in  ( SELECT de.device_note_id from dbo.udfGetProcess(  '" + uid + "' ) de ) AND dp.is_view = 1  ) AND (dp.organization_id = 0 OR dp.organization_id = '" + dvid + "')";
                    sql += " SELECT * FROM #bangTamDL1 dp where " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;


                }

                sql += "   SELECT COUNT(*) as totalRecords FROM #bangTamDL1  "
                + " SELECT COUNT(*) as totalRecordsHandover FROM #bangTamDL1 dt WHERE dt.device_process_type= -2"
                 + " SELECT COUNT(*) as totalRecordsRepair FROM #bangTamDL1  dt WHERE dt.device_process_type= 1"
                 + " SELECT COUNT(*) as totalRecordsRecall FROM #bangTamDL1 dt WHERE dt.device_process_type= 3"
                 + " SELECT COUNT(*) as totalRecordsInventory FROM #bangTamDL1 dt WHERE dt.device_process_type= 2";

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_follows_all", ip, tid, "Lỗi khi gọi Filter_device_follows_all", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_follows_all", ip, tid, "Lỗi khi gọi proc Filter_device_follows_all", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }
        #endregion
        #region Công việc

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_TaskGroups([System.Web.Mvc.Bind(Include = "Search,sqlS,sqlF,sqlO")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            try
            {
                string sqlCount = @"select count(group_id) as totalRecords from task_ca_taskgroup";
                string WhereSQL = "";
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {

                    WhereSQL += (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + " group_name like N'%" + filterSQL.Search + "%'";

                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlS))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status=" + int.Parse(filterSQL.sqlS);
                }
                else
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") + " status is not null";
                }
                if (!string.IsNullOrWhiteSpace(filterSQL.sqlF))
                {
                    WhereSQL += (WhereSQL.Trim() != "" ? " And " : " ") +
                                (super == "True" ?
                                (" organization_id = " + int.Parse(filterSQL.sqlF)) :
                                (int.Parse(filterSQL.sqlF) != 0 ? (" (organization_id= " + int.Parse(dvid) + ")") : " organization_id=0 "));
                }
                else
                {
                    WhereSQL += super == "True" ? "" : (WhereSQL.Trim() != "" ? " And " : " ") +
                                        (" (organization_id = 0 or organization_id = " + int.Parse(dvid) + ")");
                }
                if (WhereSQL.Trim() != "")
                {
                    sqlCount += " WHERE " + WhereSQL;
                    sql = @" select *, (select avatar from sys_users s WHERE s.user_id=t.created_by) as avt,
                                            (select full_name from sys_users s WHERE s.user_id=t.created_by) as fullname from task_ca_taskgroup t where " + WhereSQL;
                }
                //string OFFSET = @"(" + filterSQL.PageNo + @") * (" + filterSQL.PageSize + @")";
                string OFFSET = " ";
                sql += @" ORDER BY " + (filterSQL.sqlO != null ? filterSQL.sqlO : "is_order asc  ");
                //+ @"OFFSET " + OFFSET + " ROWS FETCH NEXT " + filterSQL.PageSize + " ROWS ONLY ";
                sql += sqlCount;
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                    domainurl + "/SQL/Filter_TaskGroup", ip, tid, "Lỗi khi gọi Filter_TaskGroup", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }),
                   domainurl + "/SQL/Filter_TaskGroup", ip, tid, "Lỗi khi gọi Filter_TaskGroup", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }
        #endregion
        #region Văn bản

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_doc_report_list_receive([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(doc_master_id)  as totalRecords from doc_master dm";
            try
            {
                //var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";


                sql = "";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " dm.organization_id is not null " : (" ( dm.organization_id = 0 or dm.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";
                        if (field.key == "dispatch_book_id")
                        {
                            sql += "  SELECT dispatch_book_id, dispatch_book_name into #SCVBC FROM doc_ca_dispatch_books  WHERE dispatch_book_id in(SELECT * FROM dbo.udf_PivotParameters('" + field.filteroperator + "', ',') upp );";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + " dm.dispatch_book_id in (select dispatch_book_id from #SCVBC)";
                        }
                        else if (field.key == "doc_group_id")
                        {
                            sql += " SELECT doc_group_id into #NhomVB FROM doc_ca_groups dcg WHERE dcg.doc_group_id IN(select * from udf_PivotParameters('" + field.filteroperator + "', ','));";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + " dm.doc_group_id in (select doc_group_id from #NhomVB)";
                        }
                        else if (field.key == "field_id")
                        {
                            sql += " SELECT field_id,field_name into #Linhvuc FROM doc_ca_fields dcf WHERE dcf.field_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ','));";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + " dm.field_name in (select field_name from #Linhvuc)";
                        }
                        else if (field.key == "department_id")
                        {
                            sql += " SELECT organization_id, organization_name into #Phongban FROM  sys_organization WHERE organization_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ','));";

                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + "  dm.department_id in (select organization_id from #Phongban)";
                        }
                        else if (field.key == "user_recever")
                        {
                            sql += "  SELECT su.user_key INTO #SysU from sys_users su WHERE su.user_id IN (select * from udf_PivotParameters('" + field.filteroperator + "', ','));" +
                                " SELECT DISTINCT dcrgu.role_group_id into #NhomNguoiDung FROM doc_ca_role_group_users dcrgu WHERE dcrgu.user_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ','));" +
                                " SELECT df.doc_master_id into #Follows from doc_follows df WHERE (df.receive_by IN (SELECT user_key FROM #SysU) AND df.receive_type =0) " +
                                " OR df.receive_type=2 OR (df.receive_by IN(SELECT role_group_id from #NhomNguoiDung) AND df.receive_type=1 ) ;";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + "    dm.doc_master_id in (select doc_master_id from #Follows)";
                        }
                        else if (field.key == "department_id_process")
                        {
                            sql += " SELECT DISTINCT so.organization_id INTO #DepartmentProcess  FROM sys_organization so   WHERE so.organization_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ',')) " +
                         " SELECT df.doc_master_id into #NguoiXuLy from doc_follows df WHERE(df.receive_by IN (SELECT organization_id FROM #DepartmentProcess) AND df.receive_type =3 ) ";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + "    dm.doc_master_id in (select doc_master_id from #NguoiXuLy)";
                        }
                        else
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
                                        WhereSQLR += " " + field.filteroperator + " ( dm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( dm." + field.key + "  <> N'" + m.value + "')";
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
                sql += " SELECT dm.doc_master_id, dm.dispatch_book_num," +
" dm.receive_date, dm.issue_place, dm.doc_code, doc_date," +
" compendium, doc_group, signer, saodv, ldt, file_name, file_path," +
"  (select dcg.doc_group_name from doc_ca_groups dcg where dcg.doc_group_id = dm.doc_group_id) as doc_group_name," +
" CASE" +
" WHEN(SELECT TOP(1) df.receive_type FROM doc_follows df WHERE df.doc_master_id = dm.doc_master_id) = 2 THEN" +
"   (SELECT TOP(1) df.receive_by_name FROM doc_follows df WHERE df.doc_master_id = dm.doc_master_id)" +
"  ELSE dm.list_receiver " +
" END as user_receive FROM doc_master dm ";
                if (WhereSQL.StartsWith("( and"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                            + " ("
                            + " (dm.compendium like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS) "
                              + " or (dm.dispatch_book_num like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS) "
                                + " or (dm.doc_code like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS) ) ";
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
                    sql += " WHERE " + WhereSQL + " and dm.nav_type=1 AND  dm.dispatch_book_id IS NOT NULL " + " and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + " and dm.nav_type=1 AND  dm.dispatch_book_id IS NOT NULL " + " and " + checkOrgz;
                }
                else if (WhereSQL.Trim() != "" && filterSQL.id == null)
                {
                    sql += " WHERE " + checkOrgz + " and dm.nav_type=1 AND  dm.dispatch_book_id IS NOT NULL " + @"
                        ORDER BY " + filterSQL.sqlO;

                    sqlCount += " WHERE " + checkOrgz + " and dm.nav_type=1 AND  dm.dispatch_book_id IS NOT NULL ";
                }
                else
                {
                    sql += " WHERE " + checkOrgz + " and dm.nav_type=1 AND  dm.dispatch_book_id IS NOT NULL " + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE " + checkOrgz + " and dm.nav_type=1 AND  dm.dispatch_book_id IS NOT NULL ";
                }
                //if (!filterSQL.next)//Đảo Sort
                //{
                //    sql = "Select * from (" + sql + ") as tbn ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC"));
                //}


                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi Filter_device_card", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi proc Filter_device_card", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_doc_report_list_internal([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(doc_master_id)  as totalRecords from doc_master dm";
            try
            {
                sql = "";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " dm.organization_id is not null " : (" ( dm.organization_id = 0 or dm.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";

                        if (field.key == "dispatch_book_id")
                        {
                            sql += "  SELECT dispatch_book_id, dispatch_book_name into #SCVBC FROM doc_ca_dispatch_books  WHERE dispatch_book_id in(SELECT * FROM dbo.udf_PivotParameters('" + field.filteroperator + "', ',') upp );";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + " dm.dispatch_book_id in (select dispatch_book_id from #SCVBC)";
                        }
                        else if (field.key == "doc_group_id")
                        {
                            sql += " SELECT doc_group_id into #NhomVB FROM doc_ca_groups dcg WHERE dcg.doc_group_id IN(select * from udf_PivotParameters('" + field.filteroperator + "', ','));";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + " dm.doc_group_id in (select doc_group_id from #NhomVB)";
                        }
                        else if (field.key == "field_id")
                        {
                            sql += " SELECT field_id,field_name into #Linhvuc FROM doc_ca_fields dcf WHERE dcf.field_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ','));";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + " dm.field_name in (select field_name from #Linhvuc)";
                        }
                        else if (field.key == "department_id")
                        {
                            sql += " SELECT organization_id, organization_name into #Phongban FROM  sys_organization WHERE organization_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ','));";

                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + "  dm.department_id in (select organization_id from #Phongban)";
                        }
                        else if (field.key == "user_recever")
                        {
                            sql += "  SELECT su.user_key INTO #SysU from sys_users su WHERE su.user_id IN (select * from udf_PivotParameters('" + field.filteroperator + "', ','));" +
                                " SELECT DISTINCT dcrgu.role_group_id into #NhomNguoiDung FROM doc_ca_role_group_users dcrgu WHERE dcrgu.user_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ','));" +
                                "SELECT df.doc_master_id into #Follows from doc_follows df WHERE (df.receive_by IN (SELECT user_key FROM #SysU) AND df.receive_type =0) " +
                                "OR df.receive_type=2 OR (df.receive_by IN(SELECT role_group_id from #NhomNguoiDung) AND df.receive_type=1 ) ;";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + "    dm.doc_master_id in (select doc_master_id from #Follows)";
                        }
                        else if (field.key == "department_id_process")
                        {
                            sql += " SELECT DISTINCT so.organization_id INTO #DepartmentProcess  FROM sys_organization so   WHERE so.organization_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ',')) " +
                         " SELECT df.doc_master_id into #NguoiXuLy from doc_follows df WHERE(df.receive_by IN (SELECT organization_id FROM #DepartmentProcess) AND df.receive_type =3 ) ";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + "    dm.doc_master_id in (select doc_master_id from #NguoiXuLy)";
                        }
                        else
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
                                        WhereSQLR += " " + field.filteroperator + " ( dm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( dm." + field.key + "  <> N'" + m.value + "')";
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

                sql += " SELECT dm.doc_master_id,dm.dispatch_book_num,dm.receive_date,dm.issue_place,dm.doc_code,doc_date,compendium,doc_group,signer,saodv,ldt,file_name," +
                        "file_path,(select dcg.doc_group_name from doc_ca_groups dcg where dcg.doc_group_id = dm.doc_group_id) as doc_group_name " + ", CASE " +
                        " WHEN(SELECT TOP(1) df.receive_type FROM doc_follows df WHERE df.doc_master_id = dm.doc_master_id) = 2 THEN (SELECT TOP(1) df.receive_by_name FROM doc_follows df WHERE df.doc_master_id = dm.doc_master_id) " +
                        "  ELSE dm.list_receiver END as user_receive" +
                        "  FROM doc_master dm";
                if (WhereSQL.StartsWith("( and"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                        + " ("
                        + " (dm.compendium like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS) "
                          + " or (dm.dispatch_book_num like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS) "
                            + " or (dm.doc_code like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS) ) ";

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
                    sql += " WHERE " + WhereSQL + " and dm.nav_type = 3  AND  dm.dispatch_book_id IS NOT NULL " + " and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + " and dm.nav_type=3  AND  dm.dispatch_book_id IS NOT NULL " + " and " + checkOrgz;
                }
                else if (WhereSQL.Trim() != "" && filterSQL.id == null)
                {
                    sql += " WHERE " + checkOrgz + " and dm.nav_type=3  AND  dm.dispatch_book_id IS NOT NULL " + @"
                        ORDER BY " + filterSQL.sqlO;

                    sqlCount += " WHERE " + checkOrgz + " and dm.nav_type=3  AND  dm.dispatch_book_id IS NOT NULL ";
                }
                else
                {
                    sql += " WHERE " + checkOrgz + " and dm.nav_type=3  AND  dm.dispatch_book_id IS NOT NULL " + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE " + checkOrgz + " and dm.nav_type=3  AND  dm.dispatch_book_id IS NOT NULL ";
                }
                //if (!filterSQL.next)//Đảo Sort
                //{
                //    sql = "Select * from (" + sql + ") as tbn ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC"));
                //}


                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi Filter_device_card", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi proc Filter_device_card", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_doc_report_list_send([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(doc_master_id)  as totalRecords from doc_master dm";
            try
            {

                sql = "";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " dm.organization_id is not null " : (" ( dm.organization_id = 0 or dm.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    var check = false;
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        string WhereSQLR = "";

                        if (field.key == "dispatch_book_id")
                        {
                            sql += "  SELECT dispatch_book_id, dispatch_book_name into #SCVBC FROM doc_ca_dispatch_books  WHERE dispatch_book_id in(SELECT * FROM dbo.udf_PivotParameters('" + field.filteroperator + "', ',') upp );";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + " dm.dispatch_book_id in (select dispatch_book_id from #SCVBC)";
                        }
                        else if (field.key == "doc_group_id")
                        {
                            sql += " SELECT doc_group_id into #NhomVB FROM doc_ca_groups dcg WHERE dcg.doc_group_id IN(select * from udf_PivotParameters('" + field.filteroperator + "', ','));";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + " dm.doc_group_id in (select doc_group_id from #NhomVB)";
                        }
                        else if (field.key == "field_id")
                        {
                            sql += " SELECT field_id,field_name into #Linhvuc FROM doc_ca_fields dcf WHERE dcf.field_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ','));";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + " dm.field_name in (select field_name from #Linhvuc)";
                        }
                        else if (field.key == "department_id")
                        {
                            sql += " SELECT organization_id, organization_name into #Phongban FROM  sys_organization WHERE organization_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ','));";

                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + "  dm.department_id in (select organization_id from #Phongban)";
                        }
                        else if (field.key == "user_recever")
                        {
                            sql += "  SELECT su.user_key INTO #SysU from sys_users su WHERE su.user_id IN (select * from udf_PivotParameters('" + field.filteroperator + "', ','));" +
                                " SELECT DISTINCT dcrgu.role_group_id into #NhomNguoiDung FROM doc_ca_role_group_users dcrgu WHERE dcrgu.user_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ','));" +
                                "SELECT df.doc_master_id into #Follows from doc_follows df WHERE (df.receive_by IN (SELECT user_key FROM #SysU) AND df.receive_type =0) " +
                                "OR df.receive_type=2 OR (df.receive_by IN(SELECT role_group_id from #NhomNguoiDung) AND df.receive_type=1   ) ;";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + "    dm.doc_master_id in (select doc_master_id from #Follows)";
                        }
                        else if (field.key == "department_id_process")
                        {
                            sql += " SELECT DISTINCT so.organization_id INTO #DepartmentProcess  FROM sys_organization so   WHERE so.organization_id in(select * from udf_PivotParameters('" + field.filteroperator + "', ',')) " +
                         " SELECT df.doc_master_id into #NguoiXuLy from doc_follows df WHERE(df.receive_by IN (SELECT organization_id FROM #DepartmentProcess) AND df.receive_type =3 ) ";
                            WhereSQL += (WhereSQL != "" ? " And " : " ");
                            WhereSQL += " " + "    dm.doc_master_id in (select doc_master_id from #NguoiXuLy)";
                        }
                        else
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
                                        WhereSQLR += " " + field.filteroperator + " ( dm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQLR += " " + field.filteroperator + " ( dm." + field.key + "  <> N'" + m.value + "')";
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

                sql += " SELECT dm.doc_master_id,dm.dispatch_book_num,dm.receive_date,dm.issue_place,dm.doc_code,doc_date,compendium,doc_group,signer,saodv,ldt,file_name,file_path,(select dcg.doc_group_name from doc_ca_groups dcg where dcg.doc_group_id = dm.doc_group_id) as doc_group_name,dm.related_unit, dm.receive_place," +

 " CASE WHEN(SELECT TOP(1) df.receive_type FROM doc_follows df WHERE df.doc_master_id = dm.doc_master_id) = 2 THEN" +
"   (SELECT TOP(1) df.receive_by_name FROM doc_follows df WHERE df.doc_master_id = dm.doc_master_id)" +
"  ELSE dm.list_receiver end " +
"   as user_receive  FROM doc_master dm";
                if (WhereSQL.StartsWith("( and"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "")
                       + " ("
                       + " (dm.compendium like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS) "
                         + " or (dm.dispatch_book_num like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS) "
                           + " or (dm.doc_code like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS) ) ";
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
                    sql += " WHERE " + WhereSQL + " and dm.nav_type=2  AND  dm.dispatch_book_id IS NOT NULL " + " and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + " and dm.nav_type=2  AND  dm.dispatch_book_id IS NOT NULL " + " and " + checkOrgz;
                }
                else if (WhereSQL.Trim() != "" && filterSQL.id == null)
                {
                    sql += " WHERE " + checkOrgz + " and dm.nav_type=2  AND  dm.dispatch_book_id IS NOT NULL " + @"
                        ORDER BY " + filterSQL.sqlO;

                    sqlCount += " WHERE " + checkOrgz + " and dm.nav_type=2  AND  dm.dispatch_book_id IS NOT NULL ";
                }
                else
                {
                    sql += " WHERE " + checkOrgz + " and dm.nav_type=2  AND  dm.dispatch_book_id IS NOT NULL " + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE " + checkOrgz + " and dm.nav_type=2  AND  dm.dispatch_book_id IS NOT NULL ";
                }
                //if (!filterSQL.next)//Đảo Sort
                //{
                //    sql = "Select * from (" + sql + ") as tbn ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC"));
                //}


                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });























            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi Filter_device_card", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi proc Filter_device_card", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Filter_doc_report_list_send_email([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(x => x.Type == "dvid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(email_sent_id)  as totalRecords from doc_email_sent des  INNER JOIN doc_master dm ON des.doc_master_id = dm.doc_master_id";
            try
            {
                var selectStr = filterSQL.id == null ? (" Select TOP(" + filterSQL.PageSize + @") ") : "Select ";


                sql = selectStr + "des.email_sent_id,des.email_name,des.doc_master_id,dm.dispatch_book_num,dm.doc_code,dm.compendium,des.title,des.content,des.sent_date,des.created_by,des.created_date," +
                    "(SELECT su.full_name FROM sys_users su WHERE su.user_id = des.created_by ) AS created_name FROM doc_email_sent des INNER JOIN doc_master dm ON des.doc_master_id = dm.doc_master_id";
                string super = claims.Where(x => x.Type == "super").FirstOrDefault()?.Value;
                string WhereSQL = "";
                string checkOrgz = super == "True" ? " dm.organization_id is not null " : (" ( dm.organization_id = 0 or dm.organization_id =" + dvid + " ) ");
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            WhereSQL += field.filterconstraints.Count > 1 ? "(" : "";
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'" + m.value + "%'";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "'";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + " like N'%" + m.value + "%'";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " " + field.key + "  not like N'%" + m.value + "%'";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " ( dm." + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                            WhereSQL += field.filterconstraints.Count > 1 ? ")" : "";
                        }
                    }
                }

                if (WhereSQL.StartsWith("( and"))
                {

                    WhereSQL = "( " + WhereSQL.Substring(5);
                }
                else if (WhereSQL.StartsWith("( or"))
                {
                    WhereSQL = "( " + WhereSQL.Substring(4);
                }
                if (WhereSQL.StartsWith(" and"))
                {

                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or"))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "( (des.email_name like N'%" + filterSQL.Search.ToUpper() + "%'  collate Latin1_General_100_CI_AS)  OR des.title like N'%" + filterSQL.Search.ToUpper() + "%'";
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
                    sql += " WHERE " + WhereSQL + " and " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;
                    sqlCount += " WHERE " + WhereSQL + " and " + checkOrgz;
                }
                else if (WhereSQL.Trim() != "" && filterSQL.id == null)
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO;

                    sqlCount += " WHERE " + checkOrgz;
                }
                else
                {
                    sql += " WHERE " + checkOrgz + @"
                        ORDER BY " + filterSQL.sqlO + offSetSQL;

                    sqlCount += " WHERE " + checkOrgz;
                }
                //if (!filterSQL.next)//Đảo Sort
                //{
                //    sql = "Select * from (" + sql + ") as tbn ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC"));
                //}


                sql += sqlCount;

                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi Filter_device_card", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/Filter_device_card", ip, tid, "Lỗi khi gọi proc Filter_device_card", 0, "SQL");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1", sql });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
        }
        #endregion

        public async Task<HttpResponseMessage> FilterSQL_file_log([System.Web.Mvc.Bind(Include = "fieldSQLS,Search,sqlO,PageNo,PageSize,next,id")][FromBody] FilterSQL filterSQL)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string rid = claims.Where(p => p.Type == "rid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            string sql = "";
            string sqlCount = " Select count(file_log_id)  as totalRecords from file_log";
            try
            {
                //int OFFSET = (filterSQL.PageNo - 1) * filterSQL.PageSize;
                sql = @"
                        Select TOP(" + filterSQL.PageSize + @") file_log_id,user_id,full_name,date_view  as is_time,w.created_ip, w.folder_id, contents,folder_name, file_name from file_log w left join file_folder f on w.folder_id = f.folder_id left join file_info i on w.file_id = i.file_id
                    ";
                string WhereSQL = "";
                if (filterSQL.fieldSQLS != null && filterSQL.fieldSQLS.Count > 0)
                {
                    foreach (var field in filterSQL.fieldSQLS)
                    {
                        if (field.filteroperator == "in")
                        {
                            WhereSQL += (WhereSQL != "" ? " And " : " ") + field.key + " in(" + String.Join(",", field.filterconstraints.Select(a => "'" + a.value + "'").ToList()) + ")";
                        }
                        else
                        {
                            foreach (var m in field.filterconstraints.Where(a => a.value != null))
                            {
                                switch (m.matchMode)
                                {
                                    case "lt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " < " + m.value + ")";
                                        break;
                                    case "gt":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >" + m.value + ")";
                                        break;
                                    case "lte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " <= " + m.value + ")";
                                        break;
                                    case "gte":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " >= " + m.value + ")";
                                        break;
                                    case "startsWith":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"" + m.value + "*\"') ";
                                        break;
                                    case "endsWith":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"*" + m.value + "\"') ";
                                        break;
                                    case "contains":
                                        WhereSQL += " " + field.filteroperator + " Contains(" + field.key + ",'\"*" + m.value + "*\"') ";
                                        break;
                                    case "notContains":
                                        WhereSQL += " " + field.filteroperator + " NOT Contains(" + field.key + ",'\"*" + m.value + "*\"') ";
                                        break;
                                    case "equals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + " = N'" + m.value + "')";
                                        break;
                                    case "notEquals":
                                        WhereSQL += " " + field.filteroperator + " (" + field.key + "  <> N'" + m.value + "')";
                                        break;
                                    case "dateIs":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) = CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateIsNot":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) <> CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateBefore":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) < CAST('" + m.value + "' as date)";
                                        break;
                                    case "dateAfter":
                                        WhereSQL += " " + field.filteroperator + " CAST(" + field.key + " as date) > CAST('" + m.value + "' as date)";
                                        break;

                                }
                            }
                        }
                    }
                }
                if (WhereSQL.StartsWith(" and "))
                {
                    WhereSQL = WhereSQL.Substring(4);
                }
                else if (WhereSQL.StartsWith(" or "))
                {
                    WhereSQL = WhereSQL.Substring(3);
                }

                if (filterSQL.next)//Trang tiếp
                {
                    if (filterSQL.id != null)
                    {
                        WhereSQL = " (file_log_id" + (filterSQL.sqlO.Contains("DESC") ? "<" : ">") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                else//Trang trước
                {
                    if (filterSQL.id != "-1")
                    {
                        WhereSQL = " (file_log_id" + (filterSQL.sqlO.Contains("DESC") ? ">" : "<") + filterSQL.id + ") " + (WhereSQL.Trim() != "" ? " And " + WhereSQL : "");
                    }
                }
                //Search
                if (!string.IsNullOrWhiteSpace(filterSQL.Search))
                {
                    WhereSQL = (WhereSQL.Trim() != "" ? (WhereSQL + " And  ") : "") + "SearchName like N'%" + filterSQL.Search.ToUpper() + "%' collate Latin1_General_100_Bin2";
                }
                if (WhereSQL.Trim() != "")
                {
                    sql += " WHERE " + WhereSQL;
                    sqlCount += " WHERE " + WhereSQL;
                }
                if (!filterSQL.next)//Đảo Sort
                {
                    sql = "Select * from (" + sql + " ORDER BY " + (filterSQL.sqlO.Contains("DESC") ? filterSQL.sqlO.Replace("DESC", "ASC") : filterSQL.sqlO.Replace("ASC", "DESC")) + ") as tbn ";
                }
                if (!ad)
                {
                    string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                    sql = @" ;With CTE as
		                    (
			                    select organization_id,parent_id from sys_organization where organization_id=" + dvid + @"
			                    union all
			                    Select a.organization_id,a.parent_id from sys_organization a inner join cte b
			                     on b.organization_id=a.parent_id 
		                    )
                            
                        " + sql + (WhereSQL.Trim() != "" ? " And " : " Where ") + @"  user_id ='" + uid + "'";
              
                    sqlCount = @" ;With CTE as
		                    (
			                    select organization_id,parent_id from sys_organization where organization_id=" + dvid + @"
			                    union all
			                    Select a.organization_id,a.parent_id from sys_organization a inner join cte b
			                     on b.organization_id=a.parent_id 
		                    )
                            
                        " + sqlCount + (WhereSQL.Trim() != "" ? " And " : " Where ") + @"  user_id ='" + uid + "'";

                }
                sql += @"
                        ORDER BY " + filterSQL.sqlO;
                if (filterSQL.id == null)
                {
                    sql += sqlCount;
                }
                sql = sql.Replace("\r", " ").Replace("\n", " ").Replace("   ", " ").Trim();
                sql = Regex.Replace(sql, @"\s+", " ").Trim();
                var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, System.Data.CommandType.Text, sql).Tables);
                var tables = await task;
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, sql, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLfile_log", ip, tid, "Lỗi khi gọi FilterSQLfile_log", 0, "SQL");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = sql, contents }), domainurl + "/SQL/FilterSQLfile_log", ip, tid, "Lỗi khi gọi proc FilterSQLfile_log", 0, "SQL");
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