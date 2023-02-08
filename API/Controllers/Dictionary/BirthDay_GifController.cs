using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
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
using OfficeOpenXml;
namespace API.Controllers.Dictionary
{
    [Authorize(Roles = "login")]
    public class BirthDay_GifController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;

        }
        [HttpPost]
        public async Task<HttpResponseMessage> AddGif()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
                try
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string strPath = root + "/Gif";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);
                    var task = Request.Content.ReadAsMultipartAsync(provider).ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        List<birthday_gif> listGif = new List<birthday_gif>();
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
                        int i = 0;
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            birthday_gif newGif = new birthday_gif();
                            string fileName = "";
                            if (string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                            {
                                fileName = Guid.NewGuid().ToString();
                            }
                            fileName = fileData.Headers.ContentDisposition.FileName;
                            if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                            {
                                fileName = fileName.Trim('"');
                            }
                            if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                            {
                                fileName = Path.GetFileName(fileName);
                            }
                            newFileName = Path.Combine(root + "/Gif", fileName);
                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;
                                newFileName = Path.Combine(root + "/Gif", fileName);
                            }
                            ffileData = fileData;
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);

                            }
                            newGif.gif = "/Portals/Gif/" + fileName;

                            newGif.created_by = uid;
                            newGif.created_date = DateTime.Now;
                            newGif.created_ip = ip;
                            newGif.created_token_ip = tid;
                            listGif.Add(newGif);

                            #region add cms_logs
                            if (helper.wlog)
                            {

                                cms_logs log = new cms_logs();
                                log.log_title = "Thêm ảnh động " + newGif.gif;
                                log.log_content = JsonConvert.SerializeObject(new { data = newGif });
                                log.log_module = "Gif";
                                log.id_key = newGif.gif_id.ToString();

                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                db.cms_logs.Add(log);
                                db.SaveChanges();
                            }
                            #endregion
                            i++;
                        }
                        if (listGif.Count > 0)
                        {
                            db.birthday_gif.AddRange(listGif);
                            db.SaveChanges();
                            errorCode = "0";
                            listErr = "";
                        }
                        else
                        {
                            listErr = "Đã xảy ra lỗi! Vui lòng kiểm tra lại";
                            errorCode = "1";
                        }

                        //Add ảnh

                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/BirthDay_GifController/AddGif", ip, tid, "Lỗi khi thêm mới Gif", 1, "birthday_gif");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/BirthDay_GifController/AddGif", ip, tid, "Lỗi khi thêm mới Gif", 1, "birthday_gif");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_Gif([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> id)
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
                    var das = await db.birthday_gif.Where(a => id.Contains(a.gif_id)).ToListAsync();
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<birthday_gif> del = new List<birthday_gif>();
                        foreach (var da in das)
                        {
                            del.Add(da);
                            if (!string.IsNullOrWhiteSpace(da.gif))
                                paths.Add(HttpContext.Current.Server.MapPath("~/Portals") + da.gif.Substring(8));

                            #region add cms_logs
                            if (helper.wlog)
                            {

                                cms_logs log = new cms_logs();
                                log.log_title = "Xóa Gif " + da.gif;

                                log.log_module = "Gif";
                                log.id_key = da.gif_id.ToString();
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
                        foreach (string strPath in paths)
                        {
                            var delPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Portals/Gif"), Path.GetFileName(strPath));
                            if (File.Exists(delPath))
                            {
                                File.Delete(delPath);
                            }
                        }
                        db.birthday_gif.RemoveRange(del);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "doc_ca_stamps/Delete_stamp", ip, tid, "Lỗi khi xoá tem", 0, "Stamp");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "doc_ca_stamps/Delete_stamp", ip, tid, "Lỗi khi xoá tem", 0, "Stamp");
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
