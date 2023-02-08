using GleamTech.DocumentUltimate.AspNet.UI;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Controllers
{
    public class ViewerController : Controller
    {
        public ActionResult Index(string url, string title)
        {
            var documentViewer = new DocumentViewer
            {

                FitMode = FitMode.FitPage,
                SidePaneVisible = false,
                Document = url,
                AllowedPermissions = DocumentViewerPermissions.All,
                DeniedPermissions =  DocumentViewerPermissions.Zoom | DocumentViewerPermissions.ViewBookmarks | DocumentViewerPermissions.Search | DocumentViewerPermissions.SelectText | DocumentViewerPermissions.Pan,
                
            };
            ViewBag.title = title ?? "Document Viewer";
            return View(documentViewer);
        }
        public void DownloadFile(string url, string title)
        {
            url = Regex.Replace(url.Replace("\\", "/"), @"\.*/+", "/");
            var listPath = url.Split('/');
            var pathFile = "";
            foreach (var item in listPath)
            {
                if (item.Trim() != "")
                {
                    pathFile += "/" + Path.GetFileName(item);
                }
            }
            FileInfo ObjArchivo = new System.IO.FileInfo(Server.MapPath("~/" + pathFile));
            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + Uri.EscapeDataString(title));
            Response.AddHeader("Content-Length", ObjArchivo.Length.ToString());
            Response.AddHeader("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            Response.ContentType = "application/pdf";
            Response.WriteFile(ObjArchivo.FullName);
            Response.End();
        }
    }
}