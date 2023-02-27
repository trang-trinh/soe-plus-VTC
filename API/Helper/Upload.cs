using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace API.Helper
{
    public class Upload
    {
        string Portals = ConfigurationManager.AppSettings["Portals"];
        public async Task UpdateFile(string jwtcookie, string root, MultipartFileData fileData, string newFileName, int? rswidth, int?rsheight)
        {
            try
            {
                string portals = "";
                bool isHttp = false;
                bool isFtp = false;
                //Portals = "ftp://123.31.12.70/PublishSOE2020/Vue2022/Code/";
                var file_size = new FileInfo(root + newFileName).Length;
                if ((fileData.Headers.ContentLength != null && fileData.Headers.ContentLength == 0) || file_size == 0)
                {
                    return;
                }
                else //if (fileData.Headers.ContentLength > 0)
                {
                    if (string.IsNullOrWhiteSpace(Portals))
                    {
                        portals = root;
                    }
                    else if (Portals.Contains("http"))
                    {
                        isHttp = true;
                        portals = Portals;
                    }
                    else if (Portals.Contains("ftp"))
                    {
                        isFtp = true;
                        portals = Portals;
                    }
                    else
                    {
                        portals = Portals + "\\Portals";
                    }
                    if (!isHttp && !isFtp)
                    {
                        newFileName = portals + newFileName.Replace("/", "\\");
                        newFileName.Replace("Portals\\Portals", "Portals");
                        string directory = Path.GetDirectoryName(newFileName);
                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);
                        File.Move(fileData.LocalFileName, newFileName);
                        if (helper.IsImageFileName(newFileName))
                        {
                            helper.ResizeImage(newFileName, 1920, 1080, 90);
                            if (rswidth != null)
                            {
                                helper.ResizeThumbImage(newFileName, rswidth ?? 160, rsheight ?? 160);
                            }
                        }
                    }
                    else if (isHttp)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            var multiForm = new MultipartFormDataContent();
                            // add API method parameters
                            multiForm.Add(new StringContent(newFileName), "newFileName");
                            if (rswidth != null)
                                multiForm.Add(new StringContent(rswidth.ToString()), "rswidth");
                            // add file and directly upload it
                            FileStream fs = File.OpenRead(fileData.LocalFileName);
                            multiForm.Add(new StreamContent(fs), "file", Path.GetFileName(fileData.LocalFileName));
                            // send request to API
                            jwtcookie = HttpUtility.UrlDecode(jwtcookie);
                            string jwt = Codec.DecryptString(jwtcookie, ConfigurationManager.AppSettings["EncriptKey"]);
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
                            var response = await client.PostAsync(portals + "/api/Upload/Update_File", multiForm);
                        }
                    }
                    else if (isFtp)
                    {
                        //string url = "ftp://" + newFileName;
                        string url = portals + newFileName;
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);

                        string json = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Config/Config.json"));
                        settings deJson = JsonConvert.DeserializeObject<settings>(json);
                        var usAccess = deJson.userftp != null ? Codec.DecryptString(deJson.userftp, helper.psKey) : "";
                        var psAccess = deJson.psftp != null ? Codec.DecryptString(deJson.psftp, helper.psKey) : "";
                        //request.Credentials = new NetworkCredential(usAccess, psAccess);
                        request.Credentials = new NetworkCredential("os", "#Os1234567BiBi");
                        request.Method = WebRequestMethods.Ftp.UploadFile;

                        FileStream fs = File.OpenRead(fileData.LocalFileName);
                        using (Stream ftpStream = request.GetRequestStream())
                        {
                            fs.CopyTo(ftpStream);
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        public async Task UpdateFilePath(string jwtcookie, string root, string filePath, string newFileName, int? rswidth)
        {
            try
            {
                string portals = "";
                bool isHttp = false;
                if (string.IsNullOrWhiteSpace(Portals))
                {
                    portals = root;
                }
                else if (Portals.Contains("http"))
                {
                    isHttp = true;
                    portals = Portals;
                }
                else
                {
                    portals = Portals + "\\Portals";
                }
                if (!isHttp)
                {
                    newFileName = portals + newFileName.Replace("/", "\\");
                    newFileName.Replace("Portals\\Portals", "Portals");
                    string directory = Path.GetDirectoryName(newFileName);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    File.Move(filePath, newFileName);
                    if (helper.IsImageFileName(newFileName))
                    {
                        helper.ResizeImage(newFileName, 1920, 1080, 90);
                        if (rswidth != null)
                        {
                            helper.ResizeThumbImage(newFileName, 160, 160);
                        }
                    }
                }
                else
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var multiForm = new MultipartFormDataContent();
                        // add API method parameters
                        multiForm.Add(new StringContent(newFileName), "newFileName");
                        if (rswidth != null)
                            multiForm.Add(new StringContent(rswidth.ToString()), "rswidth");
                        // add file and directly upload it
                        FileStream fs = File.OpenRead(filePath);
                        multiForm.Add(new StreamContent(fs), "file", Path.GetFileName(filePath));
                        // send request to API
                        jwtcookie = HttpUtility.UrlDecode(jwtcookie);
                        string jwt = Codec.DecryptString(jwtcookie, ConfigurationManager.AppSettings["EncriptKey"]);
                        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
                        var response = await client.PostAsync(portals + "/api/Upload/Update_File", multiForm);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}