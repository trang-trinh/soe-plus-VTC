using API.Models;
using GemBox.Document;
using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using System.Data.Entity;
using GleamTech.DocumentUltimate;
using Newtonsoft.Json.Linq;

namespace API.Controllers.HRM.Tool
{
    [Authorize(Roles = "login")]
    public class toolController : ApiController
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
            return HttpContext.Current.Request.UserHostAddress;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> genderHtml()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
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
                        if (!Request.Content.IsMimeMultipartContent())
                        {
                            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                        }
                        // Provider
                        string rootTemp = HttpContext.Current.Server.MapPath("~/Portals");
                        bool existsTemp = Directory.Exists(rootTemp);
                        if (!existsTemp)
                            Directory.CreateDirectory(rootTemp);
                        var provider = new MultipartFormDataStreamProvider(rootTemp);
                        var task = await Request.Content.ReadAsMultipartAsync(provider);

                        string html = "";

                        //Params
                        var usernow = await db.sys_users.FirstOrDefaultAsync(x => x.user_id == uid);
                        var type = provider.FormData.GetValues("type").SingleOrDefault();
                        if (type != null)
                        {
                            switch (type) {
                                case "1":
                                    string rootPath = HttpContext.Current.Server.MapPath("~/Portals") + "/" + usernow.organization_id + "/CVHTML/So yeu ly lich Mau 2C TCTW-98.doc";
                                    var documentConverter = new DocumentConverter(rootPath);
                                    if (documentConverter.CanConvertTo(GleamTech.DocumentUltimate.DocumentFormat.Html))
                                    {
                                        var result = documentConverter.ConvertTo(GleamTech.DocumentUltimate.DocumentFormat.Html);
                                        html = System.IO.File.ReadAllText(result.OutputFiles[0]);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            string root = HttpContext.Current.Server.MapPath("~/Portals");
                            string path = root + "/" + usernow.organization_id + "/CVHTML/" + usernow.user_id;
                            bool exists = Directory.Exists(path);
                            if (!exists)
                                Directory.CreateDirectory(path);
                            List<calendar_file> dfs = new List<calendar_file>();
                            foreach (MultipartFileData fileData in provider.FileData)
                            {
                                string org_name_file = fileData.Headers.ContentDisposition.FileName;
                                if (org_name_file.StartsWith("\"") && org_name_file.EndsWith("\""))
                                {
                                    org_name_file = org_name_file.Trim('"');
                                }
                                if (org_name_file.Contains(@"/") || org_name_file.Contains(@"\"))
                                {
                                    org_name_file = System.IO.Path.GetFileName(org_name_file);
                                }
                                string name_file = org_name_file; //helper.UniqueFileName(org_name_file);
                                string rootPath = path + "/" + name_file;
                                string Duongdan = "/Portals/" + usernow.organization_id + "/CVHTML/" + usernow.user_id + "/" + name_file;
                                string Dinhdang = helper.GetFileExtension(fileData.Headers.ContentDisposition.FileName);
                                if (rootPath.Length > 260)
                                {
                                    name_file = name_file.Substring(0, name_file.LastIndexOf('.') - 1);
                                    int le = 260 - (path.Length + 1) - Dinhdang.Length;
                                    name_file = name_file.Substring(0, le) + Dinhdang;
                                }
                                if (File.Exists(rootPath))
                                {
                                    File.Delete(rootPath);
                                }
                                File.Move(fileData.LocalFileName, rootPath);

                                var documentConverter = new DocumentConverter(rootPath);
                                if (documentConverter.CanConvertTo(GleamTech.DocumentUltimate.DocumentFormat.Html))
                                {
                                    var result = documentConverter.ConvertTo(GleamTech.DocumentUltimate.DocumentFormat.Html);
                                    html = System.IO.File.ReadAllText(result.OutputFiles[0]);
                                }
                                bool existFolder = System.IO.Directory.Exists(rootPath);
                                if (existFolder)
                                {
                                    System.IO.Directory.Delete(rootPath, true);
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { html = html, err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "tool/ExportDoc", ip, tid, "Lỗi khi export file doc", 0, "tool");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "tool/ExportDoc", ip, tid, "Lỗi khi export file doc", 0, "tool");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        public HttpResponseMessage exportDoc([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            modelHTML model = JsonConvert.DeserializeObject<modelHTML>(des);
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
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
                        var user_now = db.sys_users.AsNoTracking().FirstOrDefault(x => x.user_id == uid);
                        string rootPath = HttpContext.Current.Server.MapPath("~/Portals/" + user_now.organization_id + "/Word/");

                        // Format rootPath
                        var pathFormat_1 = Regex.Replace(rootPath.Replace("\\", "/"), @"\.*/+", "/");
                        var listPath_1 = pathFormat_1.Split('/');
                        var pathRootConfig = "";
                        var sttPartPath_1 = 1;
                        foreach (var item in listPath_1)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath_1 == 1)
                                {
                                    pathRootConfig += (item);
                                }
                                else
                                {
                                    pathRootConfig += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath_1++;
                        }
                        bool existPath = System.IO.Directory.Exists(pathRootConfig);
                        if (!existPath)
                        {
                            System.IO.Directory.CreateDirectory(pathRootConfig);
                        }
                        string path = "/Portals/" + user_now.organization_id + "/Word/" + model.name;
                        string strPath = Path.Combine(rootPath + model.name);
                        using (var htmlStream = new MemoryStream(Encoding.UTF8.GetBytes(model.html)))
                        {
                            ComponentInfo.SetLicense("DTZX-HTZ5-B7Q6-2GA6");
                            var htmlLoadOptions = new HtmlLoadOptions();
                            var document = DocumentModel.Load(htmlStream, htmlLoadOptions);
                            var opt = model.opition;
                            if (opt == null || (opt.left == 0 && opt.top == 0 && opt.right == 0 && opt.bottom == 0))
                            {
                                opt = new PDFOpition()
                                {
                                    orientation = opt.orientation ?? "Portrait",
                                    pageSize = opt.pageSize ?? "A4",
                                    left = opt.left,
                                    top = opt.top,
                                    right = opt.right,
                                    bottom = opt.bottom,
                                };
                            }
                            Section section = document.Sections[0];
                            PageSetup pageSetup = section.PageSetup;
                            PageMargins pageMargins = pageSetup.PageMargins;
                            pageMargins.Top = opt.top;
                            pageMargins.Right = opt.right;
                            pageMargins.Bottom = opt.bottom;
                            pageMargins.Left = opt.left;
                            SaveOptions opit = SaveOptions.DocxDefault;

                            // Format path strPath
                            var pathFormat = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/");
                            var listPath = pathFormat.Split('/');
                            var pathConfig = "";
                            var sttPartPath = 1;
                            foreach (var item in listPath)
                            {
                                if (item.Trim() != "")
                                {
                                    if (sttPartPath == 1)
                                    {
                                        pathConfig += (item);
                                    }
                                    else
                                    {
                                        pathConfig += "/" + Path.GetFileName(item);
                                    }
                                }
                                sttPartPath++;
                            }
                            if (File.Exists(pathConfig))
                            {
                                File.Delete(pathConfig);
                            }
                            document.Save(pathConfig, opit);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { path = path, err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "tool/ExportDoc", ip, tid, "Lỗi khi export file doc", 0, "tool");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents, contents }), domainurl + "tool/ExportDoc", ip, tid, "Lỗi khi export file doc", 0, "tool");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}
