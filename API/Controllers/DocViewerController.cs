using GleamTech.DocumentUltimate.AspNet.UI;
using System.Web.Mvc;

namespace Controllers
{
    public class DocViewerController : Controller
    {
        public ActionResult Index([System.Web.Mvc.Bind(Include = "")] string url, [System.Web.Mvc.Bind(Include = "")] string title)
        {
            var documentViewer = new DocumentViewer
            {
                ZoomLevel = 100,
                FitMode = FitMode.Zoom,
                Document = url,
                ToolbarVisible = false,
                DeniedPermissions = DocumentViewerPermissions.Zoom | DocumentViewerPermissions.ViewBookmarks | DocumentViewerPermissions.Search | DocumentViewerPermissions.SelectText | DocumentViewerPermissions.Pan,
            
            };
            ViewBag.title = title ?? "Document Viewer";
            return View(documentViewer);
        }
    }
}