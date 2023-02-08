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
using API.Helper;
using API.Models;
using Helper;
using Newtonsoft.Json;
namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class api_commentbug_emotesController : ApiController
    {


        public string getipaddress()
        {
        return  HttpContext.Current.Request.UserHostAddress;
        }


        [HttpPost]
        public async Task<HttpResponseMessage> add_emote([System.Web.Mvc.Bind(Include = "created_by,created_date,created_ip,created_token_id,modified_by,modified_date,modified_ip,modified_token_id,emote_comment_id ")] api_commentbug_emotes emote)
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
                    
                    var emoteOld = db.api_commentbug_emotes.Where(s => (s.comment_id == emote.comment_id && s.created_by ==uid)).FirstOrDefault<api_commentbug_emotes>();
                    if (emoteOld == null)
                    {
                        emote.created_by = uid;
                        emote.created_date = DateTime.Now;
                        emote.created_ip = ip;
                        emote.created_token_id = tid;
                        emote.modified_by = uid;
                        emote.modified_date = DateTime.Now;
                        emote.modified_ip = ip;
                        emote.modified_token_id = tid;
                        db.api_commentbug_emotes.Add(emote);
                        await db.SaveChangesAsync();
                        #region add cms_logs
                        if (helper.wlog)
                        {

                            cms_logs log = new cms_logs();
                            log.log_title = "Thêm biểu cảm " + emote.emote_comment_id;
                            log.log_content = JsonConvert.SerializeObject(new { data = emote });
                            log.log_module = "Biểu cảm";
                            log.id_key = emote.emote_comment_id.ToString();
                            log.created_date = DateTime.Now;
                            log.created_by = uid;
                            log.created_token_id = tid;
                            log.created_ip = ip;
                            db.cms_logs.Add(log);

                            db.SaveChanges();

                        }
                        #endregion
                    }
                    else if(emoteOld != null && emote.emote_id!=emoteOld.emote_id)
                    {
                        emoteOld.modified_by = uid;
                        emoteOld.modified_date = DateTime.Now;
                        emoteOld.modified_ip = ip;
                        emoteOld.modified_token_id = tid;
                        emoteOld.emote_id = emote.emote_id;
                        db.Entry(emoteOld).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        #region add cms_logs
                        if (helper.wlog)
                        {

                            cms_logs log = new cms_logs();
                            log.log_title = "Sửa biểu cảm " + emote.emote_comment_id;
                            log.log_content = JsonConvert.SerializeObject(new { data = emote });
                            log.log_module = "Biểu cảm";
                            log.id_key = emote.emote_comment_id.ToString();
                            log.modified_date = DateTime.Now;
                            log.modified_by = uid;
                            log.modified_token_id = tid;
                            log.modified_ip = ip;
                            db.cms_logs.Add(log);
                            db.SaveChanges();

                        }
                        #endregion
                    }
                    else
                    {
                                //#region add cms_logs
                                //if (helper.wlog)
                                //{

                                //    cms_logs log = new cms_logs();
                                //    log.log_title = "Xóa biểu cảm" + emoteOld.emote_comment_id;

                                //    log.log_module = "Biểu cảm";
                                //    log.id_key = emoteOld.emote_comment_id.ToString();
                                //    log.created_date = DateTime.Now;
                                //    log.created_by = uid;
                                //    log.created_token_id = tid;
                                //    log.created_ip = ip;
                                //    db.cms_logs.Add(log);
                                //    db.SaveChanges();

                                //}
                                //#endregion
                        db.api_commentbug_emotes.Remove(emoteOld);
                        await db.SaveChangesAsync();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "emote/Add_Tag", ip, tid, "Lỗi khi thêm nhãn", 0, "emote");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "emote/Add_Tag", ip, tid, "Lỗi khi thêm nhãn", 0, "emote");
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