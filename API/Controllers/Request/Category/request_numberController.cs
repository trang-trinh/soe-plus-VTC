using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using System.Security.Claims;
using API.Models;
using Newtonsoft.Json;
using Helper;
using System.Data.Entity.Validation;
using API.Helper;
using System.Data.Entity;

namespace API.Controllers.Request.Category
{
    [Authorize(Roles = "login")]
    public class request_numberController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]

        public async Task<HttpResponseMessage> add_request_number()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdca_number = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;

            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        fdca_number = provider.FormData.GetValues("request_number").SingleOrDefault();
                        List<request_number> models = JsonConvert.DeserializeObject<List<request_number>>(fdca_number);

                        if (models.Count > 0)
                        {
                            foreach (var item in models)
                            {
                                var setting_number = db.request_number.Where(x => x.setting_number_id == item.setting_number_id).ToList();
                                if (setting_number.Count > 0)
                                {
                                    var setting_update = db.request_number.Find(item.setting_number_id);
                                    setting_update.id_key = item.id_key;
                                    setting_update.information_column = item.information_column;
                                    setting_update.number_of_characters = item.number_of_characters;
                                    setting_update.is_use = item.is_use;
                                    setting_update.seperate = item.seperate;
                                    setting_update.automatic = item.automatic;
                                    setting_update.modified_by = uid;
                                    setting_update.modified_date = DateTime.Now;
                                    setting_update.modified_ip = ip;
                                    setting_update.modified_token_id = tid;
                                    db.Entry(setting_update).State = EntityState.Modified;
                                }
                                else
                                {
                                    item.setting_number_id = helper.GenKey();
                                    item.created_by = uid;
                                    item.created_date = DateTime.Now;
                                    item.created_ip = ip;
                                    item.created_token_id = tid;
                                    db.request_number.Add(item);
                                }
                            }
                        }

                        db.SaveChanges();


                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_number, contents }), domainurl + "request_number/add_request_number", ip, tid, "Lỗi khi thêm thiết lập số hiệu đề xuất", 0, "request_number");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdca_number, contents }), domainurl + "request_number/add_request_number", ip, tid, "Lỗi khi thêm thiết lập số hiệu đề xuất", 0, "request_number");
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
