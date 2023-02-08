using System.IO;
using System.Net.Http;
using System.Web.Mvc;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Controllers
{
    public class UploadLargeFileController : Controller
    {
        [HttpPost]
        public HttpResponseMessage UploadFile()
        {
            string dvid = Request.Form["dvid"].ToString();
            foreach (string file in Request.Files)
            {
                var FileDataContent = Request.Files[file];
                if (FileDataContent != null && FileDataContent.ContentLength > 0)
                {
                    var stream = FileDataContent.InputStream;
                    var fileName = Path.GetFileName(FileDataContent.FileName);
                    //var UploadPath = Server.MapPath("~/Portals/" + dvid + "/Videos/");
                    //Directory.CreateDirectory(UploadPath);
                    //string path = Path.Combine(UploadPath, fileName);

                    var UploadPath = "/Portals/" + dvid + "/Videos/";
                    var fileNameTemp = Regex.Replace(UploadPath.Replace("\\", "/"), @"\.*/+", "/");
                    var listPath = fileNameTemp.Split('/');
                    var pathConfig = "";
                    foreach (var item in listPath)
                    {
                        if (item.Trim() != "")
                        {
                            pathConfig += "/" + Path.GetFileName(item);
                        }
                    }
                    if (!System.IO.Directory.Exists(Server.MapPath("~/") + pathConfig)) {
                        Directory.CreateDirectory(Server.MapPath("~/") + pathConfig);
                    }
                    string path = Path.Combine(pathConfig, fileName);
                    try
                    {
                        if (System.IO.File.Exists(Server.MapPath("~/") + path))
                            System.IO.File.Delete(Server.MapPath("~/") + path);
                        using (var fileStream = System.IO.File.Create(Server.MapPath("~/") + path))
                        {
                            stream.CopyTo(fileStream);
                        }
                        // Once the file part is saved, see if we have enough to merge it
                        Shared.Utils UT = new Shared.Utils();
                        UT.MergeFile(path);
                        // System.IO.File.Delete(path);
                    }
                    catch (IOException ex)
                    {
                        // handle
                    }
                }
            }
            return new HttpResponseMessage()
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("File uploaded.")
            };
        }

    }
}