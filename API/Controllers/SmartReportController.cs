using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
//using System.Web.Mvc;
using GleamTech.DocumentUltimate;
using System.IO;
using Aspose.Cells;
using GemBox.Spreadsheet;
using System.Configuration;
using Microsoft.ApplicationBlocks.Data;
using API.Helper;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    public class SRCController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        [HttpPost]
        //public async Task<HttpResponseMessage> PostProc([FromBody] string strSQL)
        public async Task<HttpResponseMessage> PostProc([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            string strSQL = data["str"].ToObject<string>();
            strSQL = Codec.DecryptString(strSQL, ConfigurationManager.AppSettings["EncriptKey"]!.ToString());
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(strSQL)!;
            string ip = getipaddress();
            //bool Debug = ConfigurationManager.AppSettings["Debug"]!.ToString().ToLower() == "true";
            try
            {
                //Connection = configuration.GetConnectionString("dbDatabase")!;
                string Connection = ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                if (!Connection.ToLower().Contains("data source="))
                    Connection = Codec.DecryptString(Connection, ConfigurationManager.AppSettings["EKey"]!.ToString());
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
                Task<DataTableCollection> task;
                if (proc!.query)
                {
                    if (proc!.proc.ToLower().Contains("delete ") || proc!.proc.ToLower().Contains("drop ") || proc!.proc.ToLower().Contains("update ") || proc!.proc.ToLower().Contains("insert ") || proc!.proc.ToLower().Contains("--"))
                    {
                        proc!.proc = Regex.Replace(proc!.proc, "delete ", " ", RegexOptions.IgnoreCase);
                        proc!.proc = Regex.Replace(proc!.proc, "drop ", " ", RegexOptions.IgnoreCase);
                        proc!.proc = Regex.Replace(proc!.proc, "update ", " ", RegexOptions.IgnoreCase);
                        proc!.proc = Regex.Replace(proc!.proc, "insert ", " ", RegexOptions.IgnoreCase);
                        proc!.proc = Regex.Replace(proc!.proc, "--", "", RegexOptions.IgnoreCase);
                    }
                    proc!.proc = Regex.Replace(proc!.proc, "drop2table", "drop table", RegexOptions.IgnoreCase);
                    proc!.proc = Regex.Replace(proc!.proc, "create2table", "create table", RegexOptions.IgnoreCase);
                    proc!.proc = Regex.Replace(proc!.proc, "insert2into", "insert into", RegexOptions.IgnoreCase);
                    task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, CommandType.Text, proc!.proc!).Tables);
                }
                else
                {
                    task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc!.proc!, arrpas).Tables);
                }
                var tables = await task;
                DateTime edate = DateTime.Now;
                string JSONresult = JsonConvert.SerializeObject(tables);
                int time = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
                //Ghi log
                //_logger.LogInformation("[" + ip!.ToString() + "]" + proc!.proc, DateTime.UtcNow.ToLongTimeString());
                var message = "data=" + JSONresult;
                Log.Info(message);
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", data = JSONresult, proc_name = (helper.debug && proc!.query != true ? proc!.proc : ""), time });
            }
            catch (Exception e)
            {
                string StackTrace = e.StackTrace!;
                var messages = new List<string>();
                do
                {
                    messages.Add(e.Message);
                    e = e.InnerException!;
                }
                while (e != null);
                var message = string.Join("\n", messages);
                //if (!Debug) return NotFound();
                //Ghi log
                //_logger.LogInformation("[" + ip!.ToString() + "][" + proc!.proc + "][" + message + "]", DateTime.UtcNow.ToLongTimeString());
                Log.Error(message);
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", data = message });
            }
        }
                
        [HttpPost]
        public async Task<HttpResponseMessage> UpFile()
        {
            try {
                string root = HttpContext.Current.Server.MapPath("~/");
                var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                // Read the form data and return an async task.
                var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    if (provider.FileData.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Không có file up load!" });
                    }

                    List<String> filePaths = new List<string>();
                    List<String> htmls = new List<string>();
                    //foreach (IFormFile ufile in Request.Form.Files)
                    FileInfo fileInfo = null;
                    MultipartFileData ffileData = null;
                    foreach (MultipartFileData fileData in provider.FileData)
                    {

                        var html = "";
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
                       
                       
                        var extype = Path.GetExtension(fileName);
                        fileName = System.Guid.NewGuid().ToString() + extype;
                        bool isxls = fileName.ToLower().Contains(".xls");
                        var filePath = Path.Combine(root, @"/Portals", fileName);
                        var filePathHTML = filePath + ".html";

                        fileInfo = new FileInfo(filePath);
 
                        ffileData = fileData;
                        if (fileInfo != null)
                        {
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            File.Move(ffileData.LocalFileName, filePath);

                        }
              
                        if (isxls)
                        {
                            var workbook = new Workbook(filePath);
                            Aspose.Cells.HtmlSaveOptions opts = new Aspose.Cells.HtmlSaveOptions();
                            opts.ExportImagesAsBase64 = true;
                            opts.ExportActiveWorksheetOnly = true;
                            opts.WidthScalable = true;
                            workbook.Save(filePathHTML, opts);
                            html = System.IO.File.ReadAllText(filePathHTML);
                        }
                        else
                        {
                            var documentConverter = new DocumentConverter(filePath);
                            if (documentConverter.CanConvertTo(DocumentFormat.Html))
                            {
                                var rs = documentConverter.ConvertTo(DocumentFormat.Html);
                                if (!System.IO.File.Exists(filePathHTML))
                                {
                                    filePathHTML = filePathHTML.Replace(".html", "-1.html");
                                }
                                html = System.IO.File.ReadAllText(filePathHTML);
                            }
                        }
                        System.IO.File.Delete(filePathHTML);
                        System.IO.File.Delete(filePath);
                        filePaths.Add(filePath.Substring(filePath.LastIndexOf("Portals")));
                        htmls.Add(html);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", filePaths, htmls });
                });
                return await task;
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = e.ToString() });
            }
        }

        //[HttpPost("PostFile")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostSingleFile()
        {
            try
            {
                string root = HttpContext.Current.Server.MapPath("~/");
                var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                // Read the form data and return an async task.
                var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    if (provider.FileData.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Không có file up load!" });
                    }
                    List<String> htmls = new List<string>();
                    foreach (MultipartFileData ufile in provider.FileData)
                    {
                        var html = "";
                        var fileName = Path.GetFileName(ufile.Headers.ContentDisposition.FileName);
                        //var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/Portals", fileName);
                        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", fileName);
                        var filePathHTML = filePath + ".html";
                        
                        FileStream fs = File.OpenRead(ufile.LocalFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            fs.CopyTo(fileStream);
                            fileStream.Close();
                            fs.Close();
                        }

                        var documentConverter = new DocumentConverter(filePath);
                        if (documentConverter.CanConvertTo(DocumentFormat.Html))
                        {
                            var rs = documentConverter.ConvertTo(DocumentFormat.Html);
                            if (!System.IO.File.Exists(filePathHTML))
                            {
                                filePathHTML = filePathHTML.Replace(".html", "-1.html");
                            }
                            html = System.IO.File.ReadAllText(filePathHTML);
                        }
                        System.IO.File.Delete(filePath);
                        System.IO.File.Delete(filePathHTML);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", htmls });
                });
                return await task;
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = e.ToString() });
            }
        }

        //[HttpPost("PostFileXLS")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostFileXLS()
        {            
            try
            {
                string root = HttpContext.Current.Server.MapPath("~/");
                var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                // Read the form data and return an async task.
                var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    if (provider.FileData.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Không có file up load!" });
                    }
                    List<String> htmls = new List<string>();
                    FileInfo fileInfo = null;
                    MultipartFileData ffileData = null;
                    foreach (MultipartFileData fileData in provider.FileData)
                    {

                        var html = "";
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


                        var extype = Path.GetExtension(fileName);
                        fileName = System.Guid.NewGuid().ToString() + extype;
                        bool isxls = fileName.ToLower().Contains(".xls");
                        var filePath = Path.Combine(root, @"/Portals", fileName);
                        var filePathHTML = filePath + ".html";

                        fileInfo = new FileInfo(filePath);

                        ffileData = fileData;
                        if (fileInfo != null)
                        {
                            if (!Directory.Exists(fileInfo.Directory.FullName))
                            {
                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                            }
                            File.Move(ffileData.LocalFileName, filePath);

                        }
                        //FileStream fs = File.OpenRead(filePath);
                        //using (var fileStream = new FileStream(filePath, FileMode.Create))
                        //{
                        //    fs.CopyTo(fileStream);
                        //    fileStream.Close();
                        //    fs.Close();
                        //}
                        var workbook = new Workbook(filePath);
                        Aspose.Cells.HtmlSaveOptions opts = new Aspose.Cells.HtmlSaveOptions();
                        opts.ExportImagesAsBase64 = true;
                        opts.ExportActiveWorksheetOnly = true;
                        opts.WidthScalable = true;
                        workbook.Save(filePathHTML, opts);
                        html = System.IO.File.ReadAllText(filePathHTML);
                        System.IO.File.Delete(filePath);
                        System.IO.File.Delete(filePathHTML);
                        htmls.Add(html);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", htmls });
                });
                return await task;
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = e.ToString() });
            }
        }
        //[HttpPost("ConvertFile")]
        
        [HttpPost]
        //public async Task<HttpResponseMessage> ConvertFile([FromBody] string html)
        public async Task<HttpResponseMessage> ConvertFile([FromBody] JObject data)
        {
            string html = data["html"].ToObject<string>();
            string filename = data["filename"].ToObject<string>();
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    //var filePathHTML = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/Portals", "doc.html");
                    //var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/Portals", "doc.html.docx");
                    var fileName = filename + DateTime.Now.ToString("ddMMyyyy_HHmmss");
                    var filePathHTML = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", fileName + ".html");
                    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", fileName + ".html.docx");
                    System.IO.File.WriteAllText(filePathHTML, html);
                    if (DocumentConverter.CanConvert(filePathHTML, DocumentFormat.Docx))
                    {
                        DocumentConverter.Convert(filePathHTML, DocumentFormat.Docx);
                    }
                    System.IO.File.Delete(filePathHTML);
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", filePath, fileName });
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = e.ToString() });
            }
        }

        //[HttpPost("ConvertFileXLS")]
        [HttpPost]
        public async Task<HttpResponseMessage> ConvertFileXLS([FromBody] JObject data)
        {
            string html = data["html"].ToObject<string>();
            string filename = data["filename"].ToObject<string>();
            var fileName = filename + DateTime.Now.ToString("ddMMyyyy_HHmmss");
            try
            {
                //var filePathHTML = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/Portals", "doc.html");
                //var filePath1 = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/Portals", "doc1.xlsx");
                //var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/Portals", "doc.xlsx");
                var filePathHTML = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", fileName + ".html");
                var filePath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", fileName + ".xlsx");
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", fileName + ".xlsx");
                System.IO.File.WriteAllText(filePathHTML, html);
                var workbook = new Workbook(filePathHTML);
                workbook.Save(filePath1, SaveFormat.Xlsx);
                //

                FileStream fstream = new FileStream(filePath1, FileMode.Open);
                // Opening the Excel file through the file stream
                workbook = new Workbook(fstream);
                // Accessing the first worksheet in the Excel file
                Worksheet worksheet = workbook.Worksheets[0];
                // Auto-fitting the 3rd row of the worksheet
                worksheet.AutoFitRows();
                worksheet.AutoFitColumns();
                // Closing the file stream to free all resources
                fstream.Close();
                // Saving the modified Excel file
                workbook.Save(filePath);
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", filePath, fileName });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = e.ToString() });
            }
        }
        //[HttpPost("ConvertFileXLSX")]
        [HttpPost]
        //public async Task<HttpResponseMessage> ConvertFileXLSX([FromBody] string html)
        public async Task<HttpResponseMessage> ConvertFileXLSX([FromBody] JObject data)
        {
            string html = data["html"].ToObject<string>();
            string filename = data["filename"].ToObject<string>();
            var fileName = filename + DateTime.Now.ToString("ddMMyyyy_HHmmss");
            try
            {
                //var filePathHTML = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/Portals", "doc.html");
                //var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/Portals", "doc.xlsx");
                var filePathHTML = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", fileName + ".html");
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", fileName + ".xlsx");
                System.IO.File.WriteAllText(filePathHTML, html);
                SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");
                ExcelFile.Load(filePathHTML).Save(filePath);
                if (File.Exists(filePathHTML))
                {
                    File.Delete(filePathHTML);
                }
                //return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", filePath });
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", filePath, fileName });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = e.ToString() });
            }
        }

        //[HttpGet("download")]
        //public IActionResult GetBlobDownload([FromQuery] string? link)
        [HttpGet]
        public void GetBlobDownload([FromBody] string? link)
        {
            //var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/Portals", "doc.html.docx");
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", "doc.html.docx");
            var contentType = "application/vnd.ms-word";
            var fileName = "doc.docx";
            FileInfo ObjArchivo = new System.IO.FileInfo(filePath);
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            HttpContext.Current.Response.AddHeader("Content-Length", ObjArchivo.Length.ToString());
            HttpContext.Current.Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            HttpContext.Current.Response.ContentType = contentType;
            HttpContext.Current.Response.WriteFile(ObjArchivo.FullName);
            HttpContext.Current.Response.End();
            //return Request.CreateResponse(HttpStatusCode.OK, new { file = File(filePath, contentType, fileName), err = "1" });
        }

        //[HttpGet("downloadFile")]
        //public FileContentResult GetDownload([FromQuery] string? name)
        [HttpGet]
        public void GetDownload(string? name)
        {
            //var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/Portals", "doc.html.docx");
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", name);
            var contentType = "application/vnd.ms-word";
            //var myfile = System.IO.File.ReadAllBytes(filePath);
            FileInfo ObjArchivo = new System.IO.FileInfo(filePath);
            HttpContext.Current.Response.Clear();
            //HttpContext.Current.Response.AddHeader("Content", new ByteArrayContent(myfile).ToString());
            //HttpContext.Current.Response.AddHeader("Content-Length", myfile.LongLength.ToString());
            HttpContext.Current.Response.AddHeader("Content-Length", ObjArchivo.Length.ToString());
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + (name ?? "doc-edit.docx"));
            HttpContext.Current.Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            HttpContext.Current.Response.ContentType = contentType;
            //HttpContext.Current.Response.WriteFile(name ?? "doc-edit.docx");
            HttpContext.Current.Response.WriteFile(ObjArchivo.FullName);
            HttpContext.Current.Response.End();
            System.IO.File.Delete(filePath);
        }
        //[HttpGet("downloadFileXLS")]
        //public FileContentResult GetDownloadXLS([FromQuery] string? name)
        [HttpGet]
        public void GetDownloadXLS(string? name)
        {
            //var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/Portals", "doc.xlsx");
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Portals", name);
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //var myfile = System.IO.File.ReadAllBytes(filePath);
            FileInfo ObjArchivo = new System.IO.FileInfo(filePath);
            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Clear();
            //HttpContext.Current.Response.AddHeader("Content", new ByteArrayContent(myfile).ToString());
            //HttpContext.Current.Response.AddHeader("Content-Length", myfile.LongLength.ToString());
            HttpContext.Current.Response.AddHeader("Content-Length", ObjArchivo.Length.ToString());
            HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + (name ?? "doc-edit.xlsx"));
            HttpContext.Current.Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            HttpContext.Current.Response.ContentType = contentType;
            //HttpContext.Current.Response.WriteFile(name ?? "doc-edit.xlsx");
            HttpContext.Current.Response.WriteFile(ObjArchivo.FullName);
            HttpContext.Current.Response.End();
            System.IO.File.Delete(filePath);
        }


    }
}
