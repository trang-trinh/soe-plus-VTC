using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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
                string nameFileGet = root + newFileName;
                var pathFileTransfer = "";
                if (File.Exists(fileData.LocalFileName))
                {
                    pathFileTransfer = fileData.LocalFileName;
                }
                else
                {
                    pathFileTransfer = root + newFileName;
                }
                var file_up = new FileInfo(pathFileTransfer);
                //if ((fileData.Headers.ContentLength != null && fileData.Headers.ContentLength == 0) || file_size == 0)
                if (file_up.Length == 0)
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
                        if (File.Exists(fileData.LocalFileName))
                        {
                            File.Move(fileData.LocalFileName, newFileName);
                        }
                        else
                        {
                            File.Move(pathFileTransfer, newFileName);
                        }
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
                            FileStream fs = File.OpenRead(nameFileGet);
                            multiForm.Add(new StreamContent(fs), "file", Path.GetFileName(nameFileGet));
                            // send request to API
                            jwtcookie = HttpUtility.UrlDecode(jwtcookie);
                            string jwt = Codec.DecryptString(jwtcookie, ConfigurationManager.AppSettings["EncriptKey"]);
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
                            var response = await client.PostAsync(portals + "/api/Upload/Update_File", multiForm);
                            fs.Close();
                        }
                    }
                    else if (isFtp)
                    {
                        //string json = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Config/Config.json"));
                        string json = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Config/Config.json");
                        settings deJson = JsonConvert.DeserializeObject<settings>(json);
                        var usAccess = deJson.userftp != null ? Codec.DecryptString(deJson.userftp, helper.psKey) : "";
                        var psAccess = deJson.psftp != null ? Codec.DecryptString(deJson.psftp, helper.psKey) : "";

                        string url = portals + newFileName;
                        string directoryAddFtp = Path.GetDirectoryName(newFileName).Replace("\\", "/");
                        var listPartPath = Regex.Replace(directoryAddFtp.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                        var pathToDir = portals;
                        foreach (var itemPart in listPartPath)
                        {
                            var itemFormat = itemPart.Trim();
                            if (itemFormat != "")
                            {
                                try
                                {
                                    FtpWebRequest requestDir = (FtpWebRequest)WebRequest.Create(pathToDir + "/" + itemFormat);
                                    requestDir.Credentials = new NetworkCredential(usAccess, psAccess);
                                    //requestDir.KeepAlive = false;
                                    requestDir.Method = WebRequestMethods.Ftp.ListDirectory;
                                    using (FtpWebResponse makeDirResponse = (FtpWebResponse)requestDir.GetResponse())
                                    {
                                        pathToDir += "/" + itemFormat;
                                        makeDirResponse.Close();
                                    }
                                }
                                catch (WebException e)
                                {
                                    FtpWebRequest requestDir = (FtpWebRequest)WebRequest.Create(pathToDir + "/" + itemFormat);
                                    pathToDir += "/" + itemFormat;
                                    requestDir.Credentials = new NetworkCredential(usAccess, psAccess);
                                    //requestDir.KeepAlive = false;
                                    requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                                    using (FtpWebResponse makeDirResponse = (FtpWebResponse)requestDir.GetResponse())
                                    {
                                        makeDirResponse.Close();
                                    }
                                }
                            }
                        }
                        

                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                        request.Credentials = new NetworkCredential(usAccess, psAccess);
                        request.Method = WebRequestMethods.Ftp.UploadFile;

                        FileStream fs = File.OpenRead(pathFileTransfer);
                        using (Stream ftpStream = request.GetRequestStream())
                        {
                            fs.CopyTo(ftpStream);
                            ftpStream.Close();
                            fs.Close();
                        }
                    }
                }
                
            }
            catch (WebException e)
            {
                String status = ((FtpWebResponse)e.Response).StatusDescription;
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