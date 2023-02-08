using GleamTech.DocumentUltimate.AspNet.UI;
using Helper;
using System.Web.Mvc;

namespace API.Controllers.Doc
{
    public class PDFWriterController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Viewer(string url, string title)
        {
            var documentViewer = new DocumentViewer
            {
                ZoomLevel = 100,
                FitMode = FitMode.Zoom,
                Document = url,
                ToolbarVisible = false,
                DeniedPermissions = DocumentViewerPermissions.Zoom | DocumentViewerPermissions.ViewBookmarks | DocumentViewerPermissions.Search | DocumentViewerPermissions.SelectText | DocumentViewerPermissions.Pan,
                ClientEvents = new DocumentViewerClientEvents
                {
                    Loaded = "documentViewerLoaded",
                }
            };
            ViewBag.title = title ?? "Document Viewer";
            return View("~/Views/PDFWriter/Viewer.cshtml", documentViewer);
        }


        //public JsonResult SignDoc(string url, List<WaterImage> WaterImages)
        //{
        //    try
        //    {
        //        using (SOEEntities db = new SOEEntities())
        //        {
        //            var Watermarks = new List<Watermark>();
        //            foreach (var wi in WaterImages)
        //            {
        //                ImageWatermark im = new ImageWatermark
        //                {
        //                    ImageFile = helper.rootPath + wi.ImageFile,
        //                    PageRange = wi.PageRange,
        //                    HorizontalDistance = wi.HorizontalDistance * 0.75,
        //                    VerticalDistance = wi.VerticalDistance * 0.75,
        //                    Width = wi.Width * 0.75,
        //                    Height = wi.Height * 0.75,
        //                    Rotation = wi.Rotation,
        //                    Opacity = wi.Opacity
        //                };
        //                Watermarks.Add(im);
        //            }
        //            string filePath = helper.rootPath + url;
        //            var documentConverter = new DocumentConverter(filePath);
        //            PdfOutputOptions pdf = new PdfOutputOptions();
        //            pdf.Watermarks = Watermarks.ToArray();
        //            pdf.FastWebViewEnabled = true;
        //            var ext = System.IO.Path.GetExtension(url);
        //            documentConverter.ConvertTo(filePath.Replace(ext, ".pdf"), pdf);
        //            return Json(new { error = 0, url = filePath }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        string contents = helper.ExceptionMessage(e);
        //        return Json(new { error = 1, ms = contents }, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}
