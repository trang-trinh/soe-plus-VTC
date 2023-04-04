using API.Helper;
using Helper;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.Controllers
{
    //[Authorize]
    [Authorize(Roles = "login")]
    public class UploadController : ApiController
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
        [HttpPost]
        public async Task<HttpResponseMessage> Update_File()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null || uid == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                string root = HttpContext.Current.Server.MapPath("~/Portals");
                if (!Directory.Exists(root + "/Temp"))
                {
                    Directory.CreateDirectory(root + "/Temp");
                }
                var provider = new MultipartFormDataStreamProvider(root + "/Temp");

                // Read the form data and return an async task.
                var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    string newFileName = root + provider.FormData.GetValues("newFileName").SingleOrDefault();
                    string rswidth = provider.FormData.GetValues("rswidth").FirstOrDefault();
                    // This illustrates how to get thefile names.
                    foreach (MultipartFileData fileData in provider.FileData)
                    {
                        File.Move(fileData.LocalFileName, newFileName);
                        if (helper.IsImageFileName(newFileName))
                        {
                            helper.ResizeImage(newFileName, 1920, 1080, 90);
                            if (rswidth != null)
                                helper.ResizeThumbImage(newFileName, int.Parse(rswidth), int.Parse(rswidth));

                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                });
                return await task;
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Upload/Update_File", ip, tid, "Lỗi khi Upload File", 0, "Upload");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Update_FileCK()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null || uid == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}
            try
            {
                string root = HttpContext.Current.Server.MapPath("~/Portals");
                var provider = new MultipartFormDataStreamProvider(root + "/Temp");

                // Read the form data and return an async task.
                var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    string directory = root + "/ckfinder/" + uid + "/public";
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    // This illustrates how to get thefile names.
                    string fileName = "";
                    foreach (MultipartFileData fileData in provider.FileData)
                    {
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
                        var newFileName = directory + "/" + fileName;
                        if (!File.Exists(newFileName))
                        {
                            File.Move(fileData.LocalFileName, newFileName);
                            if (helper.IsImageFileName(newFileName))
                            {
                                helper.ResizeImage(newFileName, 1920, 1080, 90);
                                helper.ResizeThumbImage(newFileName, 240, 240);

                            }
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { url = "/Portals/ckfinder/" + uid + "/public/" + fileName });
                });
                return await task;
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "Upload/Update_FileCK", ip, tid, "Lỗi khi Upload File Ckeditor", 0, "Upload");
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