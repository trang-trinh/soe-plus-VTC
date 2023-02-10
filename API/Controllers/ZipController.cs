using API.Helper;
using Helper;
using Ionic.Zip;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class ZipController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        private string DataTableToJson(DataTable dataTable)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var rows = (from DataRow d in dataTable.Rows
                        select dataTable.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => d[col])).ToList();

            //rows.AddRange(from DataRow d in dataTable.Rows
            //              select dataTable.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => d[col]));
            return serializer.Serialize(rows);
        }

        [HttpPost]
        [System.Web.Mvc.ValidateInput(false)]
        public async Task<HttpResponseMessage> UpdateZip()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null || sp != true)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = sp != true ? "2" : "1" });
            }
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                string root = HttpContext.Current.Server.MapPath("~/");
                var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    var psUp = provider.FormData.GetValues("psUp").SingleOrDefault();
                    string psDecrypt = Codec.DecryptString(psUp, helper.psKey);
                    JObject jobject = JsonConvert.DeserializeObject<JObject>(psDecrypt);
                    string passW = jobject["ps"].ToObject<string>();
                    if (passW != "101219881502198921112013")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "2", ms = "Mật khẩu không đúng." });
                    }

                    var folderUp = provider.FormData.GetValues("folderUp").SingleOrDefault();
                    string folderDecrypt = Codec.DecryptString(folderUp, helper.psKey);
                    JObject jobject_1 = JsonConvert.DeserializeObject<JObject>(folderDecrypt);
                    string Folder = jobject_1["folder"].ToObject<string>();

                    var queryUp = provider.FormData.GetValues("queryUp").SingleOrDefault();
                    string queryDecrypt = Codec.DecryptString(queryUp, helper.psKey);
                    JObject jobject_2 = JsonConvert.DeserializeObject<JObject>(queryDecrypt);
                    string query = jobject_2["query"] != null && jobject_2["query"].ToObject<string>().Trim() != "" ? jobject_2["query"].ToObject<string>() : null;
                    List<string> listPathFileUp = new List<string>();

                    int rzip = -1;
                    #region Code old
                    //string pathzip = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\SFile", @"\" + Folder);
                    //pathzip = pathzip.Replace(@"\Sfile", @"\" + Folder);
                    //HttpFileCollectionBase files = Request.Files;
                    //if (files.Count > 0)
                    //{
                    //    HttpPostedFileBase f = files[0];
                    //    if (f != null)
                    //    {
                    //        using (ZipFile zip = ZipFile.Read(f.InputStream))
                    //        {
                    //            zip.ExtractAll(pathzip, ExtractExistingFileAction.OverwriteSilently);
                    //            rzip = 1;
                    //        }
                    //    }
                    //}
                    #endregion

                    var pathConfig = "";
                    if (Folder != null && Folder.Trim() != "")
                    {
                        string FolderTemp = Regex.Replace(Folder.Replace("\\", "/"), @"\.*/+", "/");
                        var listPath = FolderTemp.Split('/');
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfig += item;
                                }
                                else
                                {
                                    pathConfig += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        if (!Directory.Exists(pathConfig))
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Folder giải nén không tồn tại!", err = "2" });
                        }

                    }
                    foreach (MultipartFileData fileData in provider.FileData)
                    {
                        using (ZipFile zip = ZipFile.Read(fileData.Headers.ContentDisposition.FileName))
                        {
                            zip.ExtractAll(pathConfig, ExtractExistingFileAction.OverwriteSilently);
                            rzip = 1;
                        }
                    }
                    string rs = "";
                    string strtable = "";
                    List<string> arrtables = new List<string>();
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        string SOXPConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                        if ((query.ToLower().Contains("select ") || query.ToLower().Contains("exec ")) && !query.ToLower().Contains("proc "))
                        {
                            //var tables= SqlHelper.ExecuteDataset(SOXPConnectionString, "execQuery", query).Tables;
                            var tables = SqlHelper.ExecuteDataset(SOXPConnectionString, CommandType.Text, query).Tables;

                            foreach (DataTable table in tables)
                            {
                                strtable = DataTableToJson(table);
                                arrtables.Add(strtable);
                            }
                            rzip = 0;
                            rs = "Thực thi SQL thành công";
                        }
                        else
                        {
                            //int ec = SqlHelper.ExecuteNonQuery(SOXPConnectionString, "execQuery", query.Trim());
                            int ec = SqlHelper.ExecuteNonQuery(SOXPConnectionString, CommandType.Text, query.Trim());
                            rs = "Thực thi SQL thành công " + ec;
                        }
                    }
                    //var json = JsonConvert.SerializeObject(new { sql = rs, zip = rzip, path = pathzip, table = arrtables });
                    //return Codec.EncryptStringAES(json);

                    string JSONresult = Codec.EncryptString(JsonConvert.SerializeObject(new { sql = rs, zip = rzip, path = pathConfig, table = arrtables }), ConfigurationManager.AppSettings["EncriptKey"]);
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                });
                return await task;
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "/Zip/UpdateZip", ip, tid, "Lỗi khi update zip db", 1, "Zip");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "/Zip/UpdateZip", ip, tid, "Lỗi khi update zip db", 1, "Zip");
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
