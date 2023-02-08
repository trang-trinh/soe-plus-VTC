using Helper;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace API.Helper
{
    public class Upload
    {
        string Portals = ConfigurationManager.AppSettings["Portals"];
        public async Task UpdateFile(string jwtcookie, string root, MultipartFileData fileData, string newFileName, int? rswidth)
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
                    File.Move(fileData.LocalFileName, newFileName);
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
                        FileStream fs = File.OpenRead(fileData.LocalFileName);
                        multiForm.Add(new StreamContent(fs), "file", Path.GetFileName(fileData.LocalFileName));
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