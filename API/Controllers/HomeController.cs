using API.Models;
using GleamTech.DocumentUltimate.AspNet.UI;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult File([System.Web.Mvc.Bind(Include = "")] string url)
        {
            return File(Request.RawUrl, MimeMapping.GetMimeMapping(url));
        }
        //[OutputCache(Duration = 60 * 60 * 24, VaryByParam = "url")]
        public ActionResult ViewFile([System.Web.Mvc.Bind(Include = "")] string url)
        {
            var UrlReferrer = ConfigurationManager.AppSettings["UrlReferrer"];
            if (Request.UrlReferrer == null || Request.Cookies["jwt"] == null || (UrlReferrer != null && !Request.UrlReferrer.OriginalString.Contains(UrlReferrer)))
            {
                return View("~/Views/Viewer/404.cshtml");
            }
            url = url.Replace(' ', '+');
            var par = Codec.DecryptString(url, ConfigurationManager.AppSettings["EncriptKey"]).Split('&');
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken = null;
            ClaimsPrincipal claims = null;
            var validationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = ConfigurationManager.AppSettings["ValidIssuer"], //some string, normally web url,  
                ValidAudience = ConfigurationManager.AppSettings["ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["IssuerSigningKey"]))
            };
            //try
            //{
                string jwtcookie = Request.Cookies["jwt"].Value;
                jwtcookie = HttpUtility.UrlDecode(jwtcookie);
                string jwt = Codec.DecryptString(jwtcookie, ConfigurationManager.AppSettings["EncriptKey"]);
                claims = tokenHandler.ValidateToken(jwt, validationParameters, out validatedToken);
            //}
            //catch (SecurityTokenException)
            //{

            //}
            if (claims == null && validatedToken == null)
            {
                return View("~/Views/Viewer/404.cshtml");
            }
            bool ad = claims.FindFirst(p => p.Type == "ad")?.Value == "True";
            if (!ad)
            {
                using (DBEntities db = new DBEntities())
                {
                    string Connection = ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                    string uid = claims.FindFirst(p => p.Type == "uid")?.Value;
                    var tables = SqlHelper.ExecuteDataset(Connection, "CheckViewFile", uid, par[2], "file_info").Tables;
                    if (tables.Count == 0 || tables[0].Rows[0][0].ToString() != "True")
                    {
                        return View("~/Views/Viewer/404.cshtml");
                    }
                }
            }
            var documentViewer = new DocumentViewer
            {
                ZoomLevel = 100,
                FitMode = FitMode.Zoom,
                Document = par[0]
            };
            ViewBag.title = par[1] ?? "Document Viewer";
            return View("~/Views/Viewer/Index.cshtml", documentViewer);
        }
        //

    }
}
