using Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

namespace API
{
    public partial class FileUploadHandler : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.Page.Response.Clear();
            if (base.Request.Files["uploadfile"] != null)
            {
                this.Upload();
                
            }
            else if (base.Request.QueryString["download"] != null)
            {
                string filename = base.Server.MapPath("~/congvan.pdf");
                System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                response.ClearContent();
                response.Clear();
                response.ContentType = "application/pdf";
                response.AddHeader("Content-Disposition", "attachment; filename=" + "testfilename.pdf" + ";");
                response.TransmitFile(filename);
                response.Flush();
            }
            this.Page.Response.End();
        }

        private void Upload()
        {
            try {
                var builder = new UriBuilder(Request.Url.Scheme, Request.Url.Host, Request.Url.Port);

                HttpPostedFile file = base.Request.Files["uploadfile"];
                //string name = helper.convertToUnSign(Path.GetFileName(file.FileName));
                string name = Path.GetFileName(Regex.Replace(HttpUtility.UrlDecode(Uri.EscapeDataString(helper.convertToUnSign(file.FileName))).Replace("\\", "/"), @"\.*/+", "/"));
                string path_withoutfilename = "/Portals/TempCA";
                bool exists = Directory.Exists(base.Server.MapPath("~/" + path_withoutfilename));
                if (!exists)
                    Directory.CreateDirectory(base.Server.MapPath("~/" + path_withoutfilename));
                string fileExt = Path.GetExtension(name).ToLower();
                string uploadFilename = string.Format("{0}.signed{1}", Path.GetFileNameWithoutExtension(name), fileExt);
                string str = string.Format("{0}Upload/{1}", builder.ToString(), uploadFilename);
                bool exists_symbol = Path.GetFileNameWithoutExtension(name).Contains("casigned");
                string new_path = path_withoutfilename + '/' + Path.GetFileNameWithoutExtension(name) + (exists_symbol ? "" : "_casigned") + fileExt;
                //file.SaveAs(base.Server.MapPath("~/" + new_path));
                //this.Page.Response.Write("{\"Status\":true, \"Message\": \"\", \"FileName\": \"" + name + "\", \"FileServer\": \"" + new_path + "\"}");
                var pathSave = Regex.Replace(HttpUtility.UrlDecode(Uri.EscapeDataString(new_path)).Replace("\\", "/"), @"\.*/+", "/");
                file.SaveAs(base.Server.MapPath("~/" + pathSave));
                this.Page.Response.Write("{\"Status\":true, \"Message\": \"\", \"FileName\": \"" + HttpUtility.UrlDecode(Uri.EscapeDataString(name)) + "\", \"FileServer\": \"" + HttpUtility.UrlDecode(Uri.EscapeDataString(new_path)) + "\"}");
            }
            catch (Exception ex)
            {
                this.Page.Response.Write("{\"Status\":false, \"Message\": \""+ ex.Message + "\", \"FileName\": \"\", \"FileServer\": \"\"}");
            }
        }
    }
}