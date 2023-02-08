//using DigitalOfficePro.Html5PointSdk;
//using System.Web.Mvc;
//using System.IO;
//using System;
//namespace API.Controllers
//{
//    public class HtmlPointController : Controller
//    {
//        // GET: HtmlPoint
//        public ActionResult Index(string url,string title)
//        {
//            PresentationConverter presentationConverter = new PresentationConverter();
//            presentationConverter.Settings.CreateDirectoryForOutput = true;
//            presentationConverter.Settings.Output.AdvanceOnMouseClick = true;
//            presentationConverter.Settings.Output.BackgroundColor = 16777215;
//            presentationConverter.Settings.Output.EmbedFonts = true;
//            presentationConverter.Settings.Output.FitToWindow = true;
//            presentationConverter.Settings.Output.IncludeHiddenSlides = true;
//            presentationConverter.Settings.Output.WindowScale = 100;
//            presentationConverter.Settings.Output.SingleHtmlPerSlide =false;
//            string domainurl = HttpContext.Request.PhysicalApplicationPath;
//            string presentationName = @"E:\\QLDA.ppt";
//            presentationConverter.OpenPresentation(presentationName);
//            string outputHtmlFile = url;
//            var html5OutputFileName = Path.ChangeExtension(presentationName, "html");
//            presentationConverter.Convert(html5OutputFileName);
//            presentationConverter.ClosePresentation();
//            ViewBag.title = title ?? "HtmlPoint";

//            return View(presentationConverter);
//        }
//    }
//}