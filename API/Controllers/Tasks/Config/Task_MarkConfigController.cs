using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using System.IO;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Security.Claims;
using System.Web;
using Helper;
using API.Models;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using API.Helper;
namespace API.Controllers.Tasks.Config
{
    [Authorize(Roles = "login")]
    public class Task_MarkConfigController : ApiController
    {

        public string getipaddress()
        {
            // var host = Dns.GetHostEntry(Dns.GetHostName());
            // foreach (var ip in host.AddressList)
            // {
            //     if (ip.AddressFamily == AddressFamily.InterNetwork)
            //     {
            //         return ip.ToString();
            //     }
            // }
            // return "localhost";
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPut]
        public async Task<HttpResponseMessage> Update([System.Web.Mvc.Bind(Include = "IntID,BitTrangthai")] Trangthai trangthai)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/"; if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        var das = db.Task_Marks.Where(a => (a.mark_id == trangthai.IntID)).FirstOrDefault<Task_Marks>();
                        if (das != null)
                        {

                            das.HowToCalcMark = !trangthai.BitTrangthai;
                            das.modified_by = uid;
                            das.modified_date = DateTime.Now;
                            das.modified_ip = ip;
                            das.modified_token_id = tid;
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = das, }), domainurl + "Task_MarkConfig/Update", ip, tid, "Cập nhật cách tính điểm đánh giá", 1, "Công việc");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "Task_MarkConfig/Update", ip, tid, "Lỗi khi cập nhật cách tính điểm đánh giá", 0, "Công việc");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "taskWeights/Update_status_weights", ip, tid, "Lỗi khi cập nhật trạng thái trọng số", 0, "Công việc");
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
