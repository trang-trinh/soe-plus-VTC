using API.Helper;
using API.Models;
using GleamTech.DocumentUltimate;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.Controllers.Tivi
{
    [Authorize(Roles = "login")]
    public class TiviController : ApiController
    {
        public string getipaddress()
        {
            //var host = Dns.GetHostEntry(Dns.GetHostName());
            //foreach (var ip in host.AddressList)
            //{
            //    if (ip.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        return ip.ToString();
            //    }
            //}
            //return "localhost";
            return HttpContext.Current.Request.UserHostAddress;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> UpdateConfigTivi()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        var typeUp = provider.FormData.GetValues("typeUp").SingleOrDefault();
                        var dataConfig = provider.FormData.GetValues("modelConfig").SingleOrDefault();
                        tivi_screen_config tv_data = JsonConvert.DeserializeObject<tivi_screen_config>(dataConfig);
                        if (tv_data.organization_id == null)
                        {
                            tv_data.organization_id = user_now.organization_id;
                        }
                        if (typeUp == "Add")
                        {
                            tv_data.created_by = uid;
                            tv_data.created_date = DateTime.Now;
                            tv_data.created_ip = ip;
                            tv_data.created_token_id = tid;
                            tv_data.is_order = db.tivi_screen_config.FirstOrDefault() != null ? db.tivi_screen_config.Max(x => x.is_order) + 1 : 1;
                            db.tivi_screen_config.Add(tv_data);
                        }
                        else
                        {
                            db.Entry(tv_data).State = EntityState.Modified;
                        }
                        // Video
                        #region Video
                        //var dataVideoScreen = provider.FormData.GetValues("listVideo").SingleOrDefault();
                        //List<tivi_screen_propaganda> list_video = JsonConvert.DeserializeObject<List<tivi_screen_propaganda>>(dataVideoScreen);
                        //if (list_video != null && list_video.Count > 0)
                        //{
                        //    List<tivi_screen_propaganda> listAddVideoToDb = new List<tivi_screen_propaganda>();
                        //    var videoAdd = list_video.Where(x => db.tivi_screen_propaganda.Count(y => y.type_data == 1 && y.organization_id == user_now.organization_id && y.video_id == x.video_id) == 0).ToList();
                        //    var videoTemp = db.tivi_screen_propaganda.Where(x => x.type_data == 1 && x.organization_id == user_now.organization_id).ToList();
                        //    var videoDel = videoTemp.Where(x => list_video.Count(y => y.video_id == x.video_id) == 0).ToList();
                        //    var stt = 1;
                        //    foreach (var item in list_video)
                        //    {
                        //        var existData = videoTemp.FirstOrDefault(x => x.video_id == item.video_id);
                        //        if (existData == null)
                        //        {
                        //            tivi_screen_propaganda vd = new tivi_screen_propaganda();
                        //            vd.organization_id = user_now.organization_id;
                        //            vd.title = item.title;
                        //            vd.type_data = 1;
                        //            vd.video_id = item.video_id;
                        //            vd.link_org = item.link_org;
                        //            vd.is_embeb = item.is_embeb;
                        //            vd.created_by = uid;
                        //            vd.created_date = DateTime.Now;
                        //            vd.created_ip = ip;
                        //            vd.created_token_id = tid;
                        //            vd.is_order = stt;
                        //            listAddVideoToDb.Add(vd);
                        //        }
                        //        else
                        //        {
                        //            existData.is_order = stt;
                        //        }
                        //        stt++;
                        //    }
                        //    if (listAddVideoToDb.Count > 0)
                        //    {
                        //        db.tivi_screen_propaganda.AddRange(listAddVideoToDb);
                        //    }
                        //    if (videoDel.Count() > 0)
                        //    {
                        //        db.tivi_screen_propaganda.RemoveRange(videoDel);
                        //    }
                        //}
                        #endregion Video
                        // Shows
                        #region Shows
                        //var dataShowsScreen = provider.FormData.GetValues("listShows").SingleOrDefault();
                        //List<tivi_screen_propaganda> list_shows = JsonConvert.DeserializeObject<List<tivi_screen_propaganda>>(dataShowsScreen);
                        //if (list_shows != null && list_shows.Count > 0)
                        //{
                        //    List<tivi_screen_propaganda> listAddShowsToDb = new List<tivi_screen_propaganda>();
                        //    var showsAdd = list_shows.Where(x => db.tivi_screen_propaganda.Count(y => y.type_data == 2 && y.organization_id == user_now.organization_id && y.shows_id == x.shows_id) == 0).ToList();
                        //    var showsTemp = db.tivi_screen_propaganda.Where(x => x.type_data == 2 && x.organization_id == user_now.organization_id).ToList();
                        //    var showsDel = showsTemp.Where(x => list_shows.Count(y => y.shows_id == x.shows_id) == 0).ToList();
                        //    var stt = 1;
                        //    foreach (var item in list_shows)
                        //    {
                        //        var existData = showsTemp.FirstOrDefault(x => x.shows_id == item.shows_id);
                        //        if (existData == null)
                        //        {
                        //            tivi_screen_propaganda vd = new tivi_screen_propaganda();
                        //            vd.organization_id = user_now.organization_id;
                        //            vd.title = item.title;
                        //            vd.type_data = 2;
                        //            vd.shows_id = item.shows_id;
                        //            vd.link_org = item.link_org;
                        //            vd.is_embeb = false;
                        //            vd.created_by = uid;
                        //            vd.created_date = DateTime.Now;
                        //            vd.created_ip = ip;
                        //            vd.created_token_id = tid;
                        //            vd.is_order = stt;
                        //            if (item.link_org != null)
                        //            {
                        //                var pathFolderFile = root + "/Portals/" + (item.organization_id != null ? item.organization_id : user_now.organization_id) + "/Shows";
                        //                var pathConvertSVG = pathFolderFile + "/ConvertSVG/Pp-svg-" + item.shows_id;
                        //                if (Directory.Exists(pathConvertSVG))
                        //                {
                        //                    Directory.Delete(pathConvertSVG, true);
                        //                }
                        //                Directory.CreateDirectory(pathConvertSVG);
                        //                var pathFile = Path.Combine(pathFolderFile, Path.GetFileName(item.link_org));
                        //                if (File.Exists(pathFile))
                        //                {
                        //                    var documentConverter = new DocumentConverter(pathFile);
                        //                    if (documentConverter.CanConvertTo(DocumentFormat.Svg))
                        //                    {
                        //                        var result = documentConverter.ConvertTo((pathConvertSVG + "/" + "slide.svg"), DocumentFormat.Svg);
                        //                        foreach (var filesvg in result.OutputFiles)
                        //                        {
                        //                            var htmlSVG = System.IO.File.ReadAllText(filesvg);
                        //                            if (htmlSVG.Contains("<svg"))
                        //                            {
                        //                                var indexStartSearch = htmlSVG.IndexOf("<svg");
                        //                                var indexEndSearch = htmlSVG.IndexOf(">", indexStartSearch);
                        //                                var htmlPart1 = htmlSVG.Substring(0, indexStartSearch);
                        //                                var htmlPartEdit = htmlSVG.Substring(indexStartSearch, indexEndSearch - indexStartSearch + 1);
                        //                                var htmlPart2 = htmlSVG.Substring(indexEndSearch+1);
                        //                                htmlPartEdit = htmlPartEdit.Replace("width", "widthOld").Replace("height", "heightOld");
                        //                                htmlSVG = htmlPart1 + htmlPartEdit + htmlPart2;
                        //                                System.IO.File.WriteAllText(filesvg, htmlSVG);
                        //                            }
                        //                        }
                        //                    }
                        //                }
                        //            }
                        //            vd.link_folder = item.link_folder;
                        //            listAddShowsToDb.Add(vd);
                        //        }
                        //        else
                        //        {
                        //            existData.is_order = stt;
                        //        }
                        //        stt++;
                        //    }
                        //    if (listAddShowsToDb.Count > 0)
                        //    {
                        //        db.tivi_screen_propaganda.AddRange(listAddShowsToDb);
                        //    }
                        //    if (showsDel.Count() > 0)
                        //    {
                        //        db.tivi_screen_propaganda.RemoveRange(showsDel);
                        //    }
                        //}
                        #endregion

                        // Files
                        var dataFilesScreen = provider.FormData.GetValues("listFiles").SingleOrDefault();
                        List<tivi_screen_propaganda> list_files = JsonConvert.DeserializeObject<List<tivi_screen_propaganda>>(dataFilesScreen);

                        List<tivi_screen_propaganda> listAddFilesToDb = new List<tivi_screen_propaganda>();
                        var filesAdd = list_files.Where(x => db.tivi_screen_propaganda.Count(y => y.organization_id == user_now.organization_id && (y.video_id == x.video_id || y.shows_id == x.shows_id)) == 0).ToList();
                        var filesTemp = db.tivi_screen_propaganda.AsNoTracking().Where(x => x.organization_id == user_now.organization_id).ToList();
                        var filesDel = filesTemp.Where(x => list_files.Count(y => (y.video_id == x.video_id && x.shows_id == null) || (y.shows_id == x.shows_id && x.video_id == null)) == 0).ToList();
                        var sttFile = 1;
                        foreach (var item in list_files)
                        {
                            var existData = db.tivi_screen_propaganda.FirstOrDefault(x => x.organization_id == user_now.organization_id && ((x.video_id == item.video_id && x.shows_id == null) || (x.shows_id == item.shows_id && x.video_id == null)));
                            if (existData == null)
                            {
                                tivi_screen_propaganda vd = new tivi_screen_propaganda();
                                vd.tivi_propaganda_id = helper.GenKey();
                                vd.organization_id = user_now.organization_id;
                                vd.title = item.title;
                                vd.type_data = item.type_data == 3 ? 3 : (item.video_id != null ? 1 : 2);
                                vd.video_id = item.video_id;
                                vd.shows_id = item.shows_id;
                                vd.link_org = item.link_org;
                                vd.is_embeb = item.is_embeb;
                                vd.created_by = uid;
                                vd.created_date = DateTime.Now;
                                vd.created_ip = ip;
                                vd.created_token_id = tid;
                                vd.is_order = sttFile;
                                if (item.link_org != null && vd.type_data == 2)
                                {
                                    var pathFolderFile = root + "/Portals/" + (item.organization_id != null ? item.organization_id : user_now.organization_id) + "/Shows";
                                    #region Convert SVG
                                    //var pathConvertSVG = pathFolderFile + "/ConvertSVG/Pp-svg-" + item.shows_id;
                                    //if (Directory.Exists(pathConvertSVG))
                                    //{
                                    //    Directory.Delete(pathConvertSVG, true);
                                    //}
                                    //Directory.CreateDirectory(pathConvertSVG);
                                    //item.link_folder = "/Portals/" + (item.organization_id != null ? item.organization_id : user_now.organization_id) + "/Shows/ConvertSVG/Pp-svg-" + item.shows_id;
                                    //var pathFile = Path.Combine(pathFolderFile, Path.GetFileName(item.link_org));
                                    //if (File.Exists(pathFile))
                                    //{
                                    //    var documentConverter = new DocumentConverter(pathFile);
                                    //    if (documentConverter.CanConvertTo(DocumentFormat.Svg))
                                    //    {
                                    //        var result = documentConverter.ConvertTo((pathConvertSVG + "/" + "slide.svg"), DocumentFormat.Svg);
                                    //        vd.number_slide = result.OutputFiles.Count();
                                    //        foreach (var filesvg in result.OutputFiles)
                                    //        {
                                    //            var htmlSVG = System.IO.File.ReadAllText(filesvg);
                                    //            if (htmlSVG.Contains("<svg"))
                                    //            {
                                    //                var indexStartSearch = htmlSVG.IndexOf("<svg");
                                    //                var indexEndSearch = htmlSVG.IndexOf(">", indexStartSearch);
                                    //                var htmlPart1 = htmlSVG.Substring(0, indexStartSearch);
                                    //                var htmlPartEdit = htmlSVG.Substring(indexStartSearch, indexEndSearch - indexStartSearch + 1);
                                    //                var htmlPart2 = htmlSVG.Substring(indexEndSearch + 1);
                                    //                htmlPartEdit = htmlPartEdit.Replace("width", "widthOld").Replace("height", "heightOld");
                                    //                htmlSVG = htmlPart1 + htmlPartEdit + htmlPart2;
                                    //                System.IO.File.WriteAllText(filesvg, htmlSVG);
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    #endregion
                                    #region Convert Image PNG
                                    var pathConvertIMG = pathFolderFile + "/ConvertIMG/Pp-img-" + item.shows_id;
                                    if (Directory.Exists(pathConvertIMG))
                                    {
                                        Directory.Delete(pathConvertIMG, true);
                                    }
                                    Directory.CreateDirectory(pathConvertIMG);
                                    item.link_folder = "/Portals/" + (item.organization_id != null ? item.organization_id : user_now.organization_id) + "/Shows/ConvertIMG/Pp-img-" + item.shows_id;
                                    var pathFile = Path.Combine(pathFolderFile, Path.GetFileName(item.link_org));
                                    if (File.Exists(pathFile))
                                    {
                                        var documentConverter = new DocumentConverter(pathFile);
                                        if (documentConverter.CanConvertTo(DocumentFormat.Png))
                                        {
                                            var result = documentConverter.ConvertTo((pathConvertIMG + "/" + "slide.png"), DocumentFormat.Png);
                                            vd.number_slide = result.OutputFiles.Count();
                                            // Resize to 4K (width=3840)
                                            #region Resize to size 4K
                                            //foreach (var fileimg in result.OutputFiles)
                                            //{
                                            //    var pathResize = fileimg;
                                            //    Image image = Image.FromFile(pathResize);
                                            //    int originalWidth = image.Width;
                                            //    int originalHeight = image.Height;
                                            //    var newWidth = 3840;
                                            //    var newHeight = (newWidth / originalWidth * originalHeight);

                                            //    Bitmap newImage = new Bitmap(newWidth, newHeight);
                                            //    using (Graphics g = Graphics.FromImage(newImage))
                                            //    {
                                            //        g.DrawImage(image, 0, 0, newWidth, newHeight);
                                            //    }
                                            //    image.Dispose();
                                            //    newImage.Save(pathResize, ImageFormat.Png);
                                            //}
                                            #endregion
                                        }
                                    }
                                    #endregion
                                }
                                vd.link_folder = item.link_folder;

                                listAddFilesToDb.Add(vd);
                            }
                            else
                            {
                                existData.is_order = sttFile;
                            }
                            sttFile++;
                        }
                        if (listAddFilesToDb.Count > 0)
                        {
                            db.tivi_screen_propaganda.AddRange(listAddFilesToDb);
                        }
                        List<tivi_screen_propaganda> delData = new List<tivi_screen_propaganda>();
                        if (filesDel.Count() > 0)
                        {
                            foreach (var fileD in filesDel)
                            {
                                var da = db.tivi_screen_propaganda.FirstOrDefault(x => x.organization_id == user_now.organization_id && ((x.video_id == fileD.video_id && x.shows_id == null) || (x.shows_id == fileD.shows_id && x.video_id == null)));
                                if (da != null)
                                {
                                    delData.Add(da);
                                }
                            }
                            if (delData.Count > 0)
                            {
                                db.tivi_screen_propaganda.RemoveRange(delData);
                            }
                        }
                        // Users
                        var dataUser = provider.FormData.GetValues("listUser").SingleOrDefault();
                        List<tivi_user_access> list_user = JsonConvert.DeserializeObject<List<tivi_user_access>>(dataUser);
                        List<tivi_user_access> listAddUserToDb = new List<tivi_user_access>();
                        var userAdd = list_user.Where(x => db.tivi_user_access.Count(y => y.organization_id == user_now.organization_id && y.user_id == x.user_id) == 0).ToList();
                        var userTemp = db.tivi_user_access.AsNoTracking().Where(x => x.organization_id == user_now.organization_id).ToList();
                        var userDel = userTemp.Where(x => list_user.Count(y => y.user_id == x.user_id) == 0).ToList();
                        var stt = 1;
                        foreach (var item in list_user)
                        {
                            var existData = db.tivi_user_access.FirstOrDefault(x => x.user_id == item.user_id);
                            if (existData == null)
                            {
                                tivi_user_access vd = new tivi_user_access();
                                vd.organization_id = user_now.organization_id;
                                vd.user_id = item.user_id;
                                vd.created_by = uid;
                                vd.created_date = DateTime.Now;
                                vd.created_ip = ip;
                                vd.created_token_id = tid;
                                vd.is_order = stt;
                                vd.is_access_insurance = item.is_access_insurance;
                                vd.is_access_docs = item.is_access_docs;
                                listAddUserToDb.Add(vd);
                            }
                            else
                            {
                                existData.is_order = stt;
                                existData.is_access_insurance = item.is_access_insurance;
                                existData.is_access_docs = item.is_access_docs;
                            }
                            stt++;
                        }
                        if (listAddUserToDb.Count > 0)
                        {
                            db.tivi_user_access.AddRange(listAddUserToDb);
                        }
                        if (userDel.Count() > 0)
                        {
                            List<tivi_user_access> delUser = new List<tivi_user_access>();
                            foreach (var userD in userDel)
                            {
                                var da = db.tivi_user_access.FirstOrDefault(x => x.user_id == userD.user_id);
                                delUser.Add(da);
                            }
                            if (delUser.Count > 0)
                            {
                                db.tivi_user_access.RemoveRange(delUser);
                            }
                        }

                        // Image
                        var dataImg = provider.FormData.GetValues("listImg").SingleOrDefault();
                        List<tivi_screen_image> list_img = JsonConvert.DeserializeObject<List<tivi_screen_image>>(dataImg);
                        //List<tivi_screen_image> listAddImgToDb = new List<tivi_screen_image>();
                        //var imageAdd = list_img.Where(x => db.tivi_screen_image.Count(y => y.organization_id == user_now.organization_id && y.tivi_image_id == x.tivi_image_id) == 0).ToList();
                        var imageTemp = db.tivi_screen_image.AsNoTracking().Where(x => x.organization_id == user_now.organization_id).ToList();
                        var imageDel = imageTemp.Where(x => list_img.Count(y => y.tivi_image_id == x.tivi_image_id) == 0).ToList();
                        var sttImg = 1;
                        foreach (var item in list_img)
                        {
                            var existData = db.tivi_screen_image.FirstOrDefault(x => x.tivi_image_id == item.tivi_image_id);
                            if (existData != null)
                            {
                                existData.is_order = sttImg;
                                existData.is_active = item.is_active;
                            }
                            sttImg++;
                        }
                        List<string> pathImgDel = new List<string>();
                        if (imageDel.Count() > 0)
                        {
                            List<tivi_screen_image> delImg = new List<tivi_screen_image>();
                            foreach (var imgD in imageDel)
                            {
                                var da = db.tivi_screen_image.FirstOrDefault(x => x.tivi_image_id == imgD.tivi_image_id);
                                if (da != null) {
                                    if (da.file_path != null) {
                                        pathImgDel.Add(da.file_path);
                                    }
                                    delImg.Add(da);
                                }
                            }
                            if (delImg.Count > 0)
                            {
                                db.tivi_screen_image.RemoveRange(delImg);
                            }
                        }
                        //
                        #region log tivi
                        tivi_log logtivi = new tivi_log();
                        logtivi.tivi_id = "";
                        logtivi.method = "HttpPost";
                        logtivi.organization_id = user_now.organization_id;
                        logtivi.action = "Tivi/UpdateConfigTivi";
                        logtivi.created_by = uid;
                        logtivi.created_date = DateTime.Now;
                        logtivi.created_ip = ip;
                        logtivi.created_token_id = tid;
                        logtivi.content = "";
                        db.tivi_log.Add(logtivi);
                        #endregion

                        db.SaveChanges();
                        //if (delData.Count > 0)
                        //{
                        //    foreach (var delD in delData)
                        //    {
                        //        if (delD.type_data == 2 && delD.link_org != null)
                        //        {
                        //            var ShowOrgFile = db.shows_main.FirstOrDefault(x => x.shows_id == delD.shows_id);
                        //            if (ShowOrgFile != null)
                        //            {
                        //                var idOrgFile = ShowOrgFile.organization_id.ToString();
                        //                var pathDel = Path.Combine(root, "Portals", Path.GetFileName(idOrgFile), "Shows/ConvertIMG", Path.GetFileName("Pp-img-" + ShowOrgFile.shows_id.ToString()));
                        //                bool exists = System.IO.Directory.Exists(pathDel);
                        //                if (exists)
                        //                    System.IO.Directory.Delete(pathDel, true);
                        //            }
                        //        }
                        //    }
                        //}
                        if (pathImgDel.Count > 0)
                        {
                            foreach (var pathDel in pathImgDel)
                            {
                                var url = Regex.Replace(pathDel.Replace("\\", "/"), @"\.*/+", "/");
                                var listPath = url.Split('/');
                                var pathFile = "";
                                foreach (var item in listPath)
                                {
                                    if (item.Trim() != "")
                                    {
                                        pathFile += "/" + Path.GetFileName(item);
                                    }
                                }
                                if (File.Exists(root + pathFile))
                                {
                                    File.Delete(root + pathFile);
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/UpdateConfigTivi", ip, tid, "Lỗi khi cập nhật config màn hình TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/UpdateConfigTivi", ip, tid, "Lỗi khi cập nhật config màn hình TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> UpdateConfigUser()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        // Users
                        var dataUser = provider.FormData.GetValues("listUser").SingleOrDefault();
                        List<tivi_user_access> list_user = JsonConvert.DeserializeObject<List<tivi_user_access>>(dataUser);
                        List<tivi_user_access> listAddUserToDb = new List<tivi_user_access>();
                        var userAdd = list_user.Where(x => db.tivi_user_access.Count(y => (y.organization_id == user_now.organization_id || user_now.is_super == true) && y.user_id == x.user_id) == 0).ToList();
                        var userTemp = db.tivi_user_access.AsNoTracking().Where(x => x.organization_id == user_now.organization_id || user_now.is_super == true).ToList();
                        var userDel = userTemp.Where(x => list_user.Count(y => y.user_id == x.user_id) == 0).ToList();
                        var stt = 1;
                        foreach (var item in list_user)
                        {
                            var existData = db.tivi_user_access.FirstOrDefault(x => x.user_id == item.user_id);
                            if (existData == null)
                            {
                                tivi_user_access vd = new tivi_user_access();
                                vd.organization_id = user_now.organization_id;
                                vd.user_id = item.user_id;
                                vd.created_by = uid;
                                vd.created_date = DateTime.Now;
                                vd.created_ip = ip;
                                vd.created_token_id = tid;
                                vd.is_order = stt;
                                vd.is_access_insurance = item.is_access_insurance;
                                vd.is_access_docs = item.is_access_docs;
                                listAddUserToDb.Add(vd);
                            }
                            else
                            {
                                existData.is_order = stt;
                                existData.is_access_insurance = item.is_access_insurance;
                                existData.is_access_docs = item.is_access_docs;
                            }
                            stt++;
                        }
                        if (listAddUserToDb.Count > 0)
                        {
                            db.tivi_user_access.AddRange(listAddUserToDb);
                        }
                        if (userDel.Count() > 0)
                        {
                            List<tivi_user_access> delUser = new List<tivi_user_access>();
                            foreach (var userD in userDel)
                            {
                                var da = db.tivi_user_access.FirstOrDefault(x => x.user_id == userD.user_id);
                                delUser.Add(da);
                            }
                            if (delUser.Count > 0)
                            {
                                db.tivi_user_access.RemoveRange(delUser);
                            }
                        }

                        #region log tivi
                        tivi_log logtivi = new tivi_log();
                        logtivi.tivi_id = "";
                        logtivi.method = "HttpPost";
                        logtivi.organization_id = user_now.organization_id;
                        logtivi.action = "Tivi/UpdateConfigUser";
                        logtivi.created_by = uid;
                        logtivi.created_date = DateTime.Now;
                        logtivi.created_ip = ip;
                        logtivi.created_token_id = tid;
                        logtivi.content = "";
                        db.tivi_log.Add(logtivi);
                        #endregion
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/UpdateConfigUser", ip, tid, "Lỗi khi cập nhật config người dùng TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/UpdateConfigUser", ip, tid, "Lỗi khi cập nhật config người dùng TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> UploadImageTivi()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
                    }
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";

                        var numfileImg = db.tivi_screen_image.Count(x => x.organization_id == user_now.organization_id);
                        List<tivi_screen_image> listFileUp = new List<tivi_screen_image>();
                        List<string> listPathFileUp = new List<string>();
                        foreach (MultipartFileData fileData in provider.FileData)
                        {
                            numfileImg++;
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
                            newFileName = Path.Combine(root + "/Portals/" + organization_id_user + "/ConfigTivi", fileName);
                            fileInfo = new FileInfo(newFileName);
                            Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;
                                // Convert to unsign                                
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");
                            }
                            else
                            {
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), ""); ;
                            }
                            newFileName = Path.Combine(root + "/Portals/" + organization_id_user + "/ConfigTivi", fileName);
                            tivi_screen_image file_img = new tivi_screen_image();
                            file_img.tivi_image_id = helper.GenKey();
                            file_img.file_path = "/Portals/" + organization_id_user + "/ConfigTivi/" + fileName;
                            file_img.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                            file_img.file_size = new FileInfo(fileData.LocalFileName).Length;
                            file_img.file_type = file_img.file_name.Substring(file_img.file_name.LastIndexOf(".") + 1).ToLower();
                            file_img.is_order = numfileImg;
                            file_img.created_by = uid;
                            file_img.created_date = DateTime.Now;
                            file_img.created_ip = ip;
                            file_img.created_token_id = tid;
                            file_img.is_active = true;
                            file_img.organization_id = user_now.organization_id;
                            listFileUp.Add(file_img);
                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                if (!Directory.Exists(fileInfo.Directory.FullName))
                                {
                                    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                }
                                File.Move(ffileData.LocalFileName, newFileName);
                                helper.ResizeImage(newFileName, 3840, 2160, 90); // 4K
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }
                        if (listFileUp.Count > 0)
                        {
                            db.tivi_screen_image.AddRange(listFileUp);
                        }

                        #region log tivi
                        tivi_log logtivi = new tivi_log();
                        logtivi.tivi_id = "";
                        logtivi.method = "HttpPost";
                        logtivi.organization_id = user_now.organization_id;
                        logtivi.action = "Tivi/UploadImageTivi";
                        logtivi.created_by = uid;
                        logtivi.created_date = DateTime.Now;
                        logtivi.created_ip = ip;
                        logtivi.created_token_id = tid;
                        logtivi.content = "";
                        db.tivi_log.Add(logtivi);
                        #endregion
                        db.SaveChanges();
                        if (listPathFileUp.Count > 0)
                        {
                            foreach (var path in listPathFileUp)
                            {
                                if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.Delete(path);
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/UploadImageTivi", ip, tid, "Lỗi khi upload file hình ảnh TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/UploadImageTivi", ip, tid, "Lỗi khi upload file hình ảnh TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_StatusTivi([System.Web.Mvc.Bind(Include = "TextID,BitTrangthai")] Trangthai trangthai)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var ID_Change_Status = trangthai.TextID.ToString();
                    //var das = db.tivi_screen.FirstOrDefault(a => a.tivi_id == ID_Change_Status);
                    var das = db.tivi_screen.Find(ID_Change_Status);
                    if (das != null)
                    {
                        das.is_active = !trangthai.BitTrangthai;
                        #region add cms_logs
                        if (helper.wlog)
                        {
                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { das.tivi_id, das.tivi_name, das.created_date, das.created_ip, das.created_token_id, das.is_active, das.is_order, das.is_change_screen, das.time_change_screen }), domainurl + "Tivi/Update_StatusTivi", ip, tid, "Sửa trạng thái tivi", 1, "Tivi");
                        }
                        #endregion

                        #region log tivi
                        var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                        tivi_log logtivi = new tivi_log();
                        logtivi.tivi_id = das.tivi_id;
                        logtivi.method = "HttpPut";
                        logtivi.organization_id = user_now?.organization_id;
                        logtivi.action = "Tivi/Update_StatusTivi";
                        logtivi.created_by = uid;
                        logtivi.created_date = DateTime.Now;
                        logtivi.created_ip = ip;
                        logtivi.created_token_id = tid;
                        logtivi.content = JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(das, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) });
                        db.tivi_log.Add(logtivi);
                        #endregion
                        await db.SaveChangesAsync();
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "Tivi/Update_StatusTivi", ip, tid, "Lỗi khi cập nhật trạng thái tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = trangthai.IntID, contents }), domainurl + "Tivi/Update_StatusTivi", ip, tid, "Lỗi khi cập nhật trạng thái tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Update_Tivi()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        var modelTivi = provider.FormData.GetValues("modelTivi").SingleOrDefault();
                        tivi_screen tivi = JsonConvert.DeserializeObject<tivi_screen>(modelTivi);
                        if (tivi.tivi_id != null) {
                            var dataTivi = db.tivi_screen.FirstOrDefault(x => x.tivi_id == tivi.tivi_id);
                            dataTivi.is_change_screen = tivi.is_change_screen == true ? true : false;
                            dataTivi.time_change_screen = tivi.time_change_screen;

                            List<tivi_screen_detail> list_edit_screen = new List<tivi_screen_detail>();
                            List<tivi_screen_detail> list_del_screen = new List<tivi_screen_detail>();

                            var listScreen = provider.FormData.GetValues("listScreen").SingleOrDefault();
                            List<tivi_screen_detail> screen = JsonConvert.DeserializeObject<List<tivi_screen_detail>>(listScreen);
                            var screenTemp = db.tivi_screen_detail.AsNoTracking().Where(x => x.tivi_id == dataTivi.tivi_id).ToList();
                            var screenDel = screenTemp.Where(x => screen.Count(y => y.screen_id == x.screen_id) == 0).ToList();
                            var sttScreen = 1;
                            foreach (var item in screen)
                            {
                                var existData = db.tivi_screen_detail.FirstOrDefault(x => x.screen_id == item.screen_id);
                                if (existData != null)
                                {
                                    existData.is_order = sttScreen;
                                    existData.is_active = item.is_active;
                                    list_edit_screen.Add(existData);
                                }
                                sttScreen++;
                            }
                            if (screenDel.Count() > 0)
                            {
                                List<tivi_screen_detail> delScreen = new List<tivi_screen_detail>();
                                foreach (var scrD in screenDel)
                                {
                                    var da = db.tivi_screen_detail.FirstOrDefault(x => x.screen_id == scrD.screen_id);
                                    if (da != null)
                                    {
                                        list_del_screen.Add(da);
                                        delScreen.Add(da);
                                        if (helper.wlog)
                                        {
                                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { da.screen_id, da.screen_name, da.tivi_id, da.display_screen_calendar, da.display_screen_docs, da.display_screen_shows, da.display_screen_image, da.display_calendar_meeting, da.display_calendar_working, da.display_calendar_duty, da.number_docs, da.calendar_department_id, da.is_change_shows, da.time_change_shows, da.is_change_images, da.time_change_images, da.is_order, da.is_active }), domainurl + "Tivi/Update_Tivi", ip, tid, "Xóa tivi", 1, "Tivi");
                                        }
                                    }
                                }
                                if (delScreen.Count > 0)
                                {
                                    db.tivi_screen_detail.RemoveRange(delScreen);
                                }
                            }

                            #region log tivi
                            tivi_log logtivi = new tivi_log();
                            logtivi.tivi_id = dataTivi.tivi_id;
                            logtivi.method = "HttpPost";
                            logtivi.organization_id = user_now?.organization_id;
                            logtivi.action = "Tivi/Update_Tivi";
                            logtivi.created_by = uid;
                            logtivi.created_date = DateTime.Now;
                            logtivi.created_ip = ip;
                            logtivi.created_token_id = tid;
                            logtivi.content = JsonConvert.SerializeObject(
                                new { 
                                    dataTivi = JsonConvert.SerializeObject(dataTivi, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                                    screenEdit = JsonConvert.SerializeObject(list_edit_screen, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                                    screenDel = JsonConvert.SerializeObject(list_del_screen, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) 
                            });
                            db.tivi_log.Add(logtivi);
                            #endregion
                            db.SaveChanges();
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "2", ms = "Tivi được chọn không tồn tại." });
                        }
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Update_Tivi", ip, tid, "Lỗi khi cập nhật config TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Update_Tivi", ip, tid, "Lỗi khi cập nhật config TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Update_Screen()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        var typeUp = provider.FormData.GetValues("typeUp").SingleOrDefault();
                        var screenTivi = provider.FormData.GetValues("screenTivi").SingleOrDefault();
                        tivi_screen_detail tv_screen = JsonConvert.DeserializeObject<tivi_screen_detail>(screenTivi);

                        List<tivi_screen_detail> list_edit_screen = new List<tivi_screen_detail>();
                        List<tivi_screen_detail> list_del_screen = new List<tivi_screen_detail>();

                        if (typeUp == "Add")
                        {
                            tv_screen.screen_id = helper.GenKey();
                            tv_screen.display_screen_calendar = false;
                            tv_screen.display_screen_docs = false;
                            tv_screen.display_screen_shows = false;
                            tv_screen.display_screen_image = false;
                            tv_screen.display_calendar_meeting = false;
                            tv_screen.display_calendar_working = false;
                            tv_screen.display_calendar_duty = false;
                            tv_screen.number_docs = 10;
                            tv_screen.is_change_shows = false;
                            tv_screen.time_change_shows = 20;
                            tv_screen.is_change_images = false;
                            tv_screen.time_change_images = 20;
                            tv_screen.is_active = true;
                            tv_screen.is_order = db.tivi_screen_detail.FirstOrDefault() != null ? db.tivi_screen_detail.Max(x => x.is_order) + 1 : 1;
                            db.tivi_screen_detail.Add(tv_screen);

                            // Xóa màn hình bị remove từ tivi hiện tại
                            var listScreen = provider.FormData.GetValues("listScreenTivi").SingleOrDefault();
                            List<tivi_screen_detail> screen = JsonConvert.DeserializeObject<List<tivi_screen_detail>>(listScreen);
                            var screenTemp = db.tivi_screen_detail.AsNoTracking().Where(x => x.tivi_id == tv_screen.tivi_id).ToList();
                            var screenDel = screenTemp.Where(x => screen.Count(y => y.screen_id == x.screen_id) == 0).ToList();
                            var sttScreen = 1;
                            foreach (var item in screen)
                            {
                                var existData = db.tivi_screen_detail.FirstOrDefault(x => x.screen_id == item.screen_id);
                                if (existData != null)
                                {
                                    existData.is_order = sttScreen;
                                    existData.is_active = item.is_active;
                                    list_edit_screen.Add(existData);
                                }
                                sttScreen++;
                            }
                            if (screenDel.Count() > 0)
                            {
                                List<tivi_screen_detail> delScreen = new List<tivi_screen_detail>();
                                foreach (var scrD in screenDel)
                                {
                                    var da = db.tivi_screen_detail.FirstOrDefault(x => x.screen_id == scrD.screen_id);
                                    if (da != null)
                                    {
                                        list_del_screen.Add(da);
                                        delScreen.Add(da);
                                        if (helper.wlog)
                                        {
                                            helper.saveLog(uid, name, JsonConvert.SerializeObject(new { da.screen_id, da.screen_name, da.tivi_id, da.display_screen_calendar, da.display_screen_docs, da.display_screen_shows, da.display_screen_image, da.display_calendar_meeting, da.display_calendar_working, da.display_calendar_duty, da.number_docs, da.calendar_department_id, da.is_change_shows, da.time_change_shows, da.is_change_images, da.time_change_images, da.is_order, da.is_active }), domainurl + "Tivi/Update_Screen", ip, tid, "Xóa màn hình trong khi thêm màn hình tivi", 1, "Tivi");
                                        }
                                    }
                                }
                                if (delScreen.Count > 0)
                                {
                                    db.tivi_screen_detail.RemoveRange(delScreen);
                                }
                            }

                        }
                        else
                        {
                            db.Entry(tv_screen).State = EntityState.Modified;
                        }

                        #region log tivi
                        tivi_log logtivi = new tivi_log();
                        logtivi.tivi_id = tv_screen.tivi_id;
                        logtivi.method = "HttpPost";
                        logtivi.organization_id = user_now?.organization_id;
                        logtivi.action = "Tivi/Update_Screen";
                        logtivi.created_by = uid;
                        logtivi.created_date = DateTime.Now;
                        logtivi.created_ip = ip;
                        logtivi.created_token_id = tid;
                        logtivi.content = JsonConvert.SerializeObject(
                            new { 
                                screenAdd = JsonConvert.SerializeObject(tv_screen, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                                screenEdit = JsonConvert.SerializeObject(list_edit_screen, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                                screenDel = JsonConvert.SerializeObject(list_del_screen, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) 
                            });
                        db.tivi_log.Add(logtivi);
                        #endregion
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Update_Screen", ip, tid, "Lỗi khi thêm màn hình TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Update_Screen", ip, tid, "Lỗi khi thêm màn hình TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateConfigScreen()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        var typeUp = provider.FormData.GetValues("typeUp").SingleOrDefault();
                        var dataConfig = provider.FormData.GetValues("modelConfig").SingleOrDefault();
                        tivi_screen_detail screen_data = JsonConvert.DeserializeObject<tivi_screen_detail>(dataConfig);
                        if (screen_data != null)
                        {
                            var detail_screen = db.tivi_screen_detail.FirstOrDefault(x => x.screen_id == screen_data.screen_id);
                            if (detail_screen != null)
                            {
                                detail_screen.display_screen_calendar = screen_data.display_screen_calendar == true ? true : false;
                                detail_screen.display_calendar_meeting = screen_data.display_calendar_meeting == true ? true : false;
                                detail_screen.display_calendar_working = screen_data.display_calendar_working == true ? true : false;
                                detail_screen.display_calendar_duty = screen_data.display_calendar_duty == true ? true : false;
                                detail_screen.calendar_department_id = screen_data.calendar_department_id;
                                detail_screen.display_screen_docs = screen_data.display_screen_docs == true ? true : false;
                                detail_screen.number_docs = screen_data.number_docs;
                                detail_screen.display_screen_shows = screen_data.display_screen_shows == true ? true : false;
                                detail_screen.display_screen_image = screen_data.display_screen_image == true ? true : false;
                                detail_screen.is_change_shows = screen_data.is_change_shows == true ? true : false;
                                detail_screen.time_change_shows = screen_data.time_change_shows;
                                detail_screen.is_change_images = screen_data.is_change_images == true ? true : false;
                                detail_screen.time_change_images = screen_data.time_change_images;

                                // Files
                                var dataFilesScreen = provider.FormData.GetValues("listFiles").SingleOrDefault();
                                List<tivi_screen_propaganda> list_files = JsonConvert.DeserializeObject<List<tivi_screen_propaganda>>(dataFilesScreen);

                                List<tivi_screen_propaganda> list_edit_pro = new List<tivi_screen_propaganda>();
                                List<tivi_screen_image> list_edit_image = new List<tivi_screen_image>();

                                List<tivi_screen_propaganda> listAddFilesToDb = new List<tivi_screen_propaganda>();
                                var filesAdd = list_files.Where(x => db.tivi_screen_propaganda.Count(y => y.screen_id == detail_screen.screen_id && (y.video_id == x.video_id || y.shows_id == x.shows_id)) == 0).ToList();
                                var filesTemp = db.tivi_screen_propaganda.AsNoTracking().Where(x => x.screen_id == detail_screen.screen_id).ToList();
                                var filesDel = filesTemp.Where(x => list_files.Count(y => y.tivi_propaganda_id != null && y.tivi_propaganda_id == x.tivi_propaganda_id) == 0).ToList();
                                var sttFile = 1;
                                foreach (var item in list_files)
                                {
                                    var existData = db.tivi_screen_propaganda.FirstOrDefault(x => x.screen_id == detail_screen.screen_id && ((x.video_id == item.video_id && x.shows_id == null) || (x.shows_id == item.shows_id && x.video_id == null)));
                                    if (existData == null)
                                    {
                                        tivi_screen_propaganda vd = new tivi_screen_propaganda();
                                        vd.tivi_propaganda_id = helper.GenKey();
                                        vd.organization_id = user_now.organization_id;
                                        vd.screen_id = detail_screen.screen_id;
                                        vd.title = item.title;
                                        vd.type_data = item.type_data == 3 ? 3 : (item.video_id != null ? 1 : 2);
                                        vd.video_id = item.video_id;
                                        vd.shows_id = item.shows_id;
                                        vd.link_org = item.link_org;
                                        vd.is_embeb = item.is_embeb;
                                        vd.created_by = uid;
                                        vd.created_date = DateTime.Now;
                                        vd.created_ip = ip;
                                        vd.created_token_id = tid;
                                        vd.is_order = sttFile;
                                        if (item.link_org != null && vd.type_data == 2)
                                        {
                                            var pathFolderFile = root + "/Portals/" + (item.organization_id != null ? item.organization_id : user_now.organization_id) + "/Shows";
                                            #region Convert Image PNG
                                            var pathConvertIMG = pathFolderFile + "/ConvertIMG/Pp-img-" + item.shows_id;
                                            if (Directory.Exists(pathConvertIMG))
                                            {
                                                Directory.Delete(pathConvertIMG, true);
                                            }
                                            Directory.CreateDirectory(pathConvertIMG);
                                            item.link_folder = "/Portals/" + (item.organization_id != null ? item.organization_id : user_now.organization_id) + "/Shows/ConvertIMG/Pp-img-" + item.shows_id;
                                            var pathFile = Path.Combine(pathFolderFile, Path.GetFileName(item.link_org));
                                            if (File.Exists(pathFile))
                                            {
                                                var documentConverter = new DocumentConverter(pathFile);
                                                if (documentConverter.CanConvertTo(DocumentFormat.Png))
                                                {
                                                    var result = documentConverter.ConvertTo((pathConvertIMG + "/" + "slide.png"), DocumentFormat.Png);
                                                    vd.number_slide = result.OutputFiles.Count();
                                                }
                                            }
                                            #endregion
                                        }
                                        vd.link_folder = item.link_folder;
                                        listAddFilesToDb.Add(vd);
                                    }
                                    else
                                    {
                                        list_edit_pro.Add(existData);
                                        existData.is_order = sttFile;
                                    }
                                    sttFile++;
                                }
                                if (listAddFilesToDb.Count > 0)
                                {
                                    db.tivi_screen_propaganda.AddRange(listAddFilesToDb);
                                }
                                List<tivi_screen_propaganda> delData = new List<tivi_screen_propaganda>();
                                if (filesDel.Count() > 0)
                                {
                                    foreach (var fileD in filesDel)
                                    {
                                        var da = db.tivi_screen_propaganda.FirstOrDefault(x => x.screen_id == detail_screen.screen_id && ((x.video_id == fileD.video_id && x.shows_id == null) || (x.shows_id == fileD.shows_id && x.video_id == null)));
                                        if (da != null)
                                        {
                                            delData.Add(da);
                                        }
                                    }
                                    if (delData.Count > 0)
                                    {
                                        db.tivi_screen_propaganda.RemoveRange(delData);
                                    }
                                }

                                // Image
                                var dataImg = provider.FormData.GetValues("listImg").SingleOrDefault();
                                List<tivi_screen_image> list_img = JsonConvert.DeserializeObject<List<tivi_screen_image>>(dataImg);
                                List<tivi_screen_image> listAddImagesToDb = new List<tivi_screen_image>();
                                var imageAdd = list_img.Where(x => db.tivi_screen_image.Count(y => y.screen_id == detail_screen.screen_id && ((y.tivi_image_id == x.tivi_image_id && x.is_copy == false) || ( y.group_img == x.group_img && x.is_copy == true))) == 0).ToList();
                                var imageTemp = db.tivi_screen_image.AsNoTracking().Where(x => x.screen_id == detail_screen.screen_id).ToList();
                                var imageDel = imageTemp.Where(x => list_img.Count(y => (x.tivi_image_id == y.tivi_image_id && y.is_copy == false) || (x.group_img == y.group_img && y.is_copy == true)) == 0).ToList();
                                var sttImg = 1;
                                foreach (var item in list_img)
                                {
                                    var existData = db.tivi_screen_image.FirstOrDefault(x => (x.tivi_image_id == item.tivi_image_id && item.is_copy == false) || (x.group_img == item.group_img && item.is_copy == true));
                                    if (existData != null)
                                    {
                                        list_edit_image.Add(existData);
                                        existData.is_order = sttImg;
                                        existData.is_active = item.is_active;
                                    }
                                    else
                                    {
                                        tivi_screen_image vd = new tivi_screen_image();
                                        vd.organization_id = user_now.organization_id;
                                        vd.screen_id = detail_screen.screen_id;
                                        vd.file_name = item.file_name;
                                        vd.file_path = item.file_path;
                                        vd.file_size = item.file_size;
                                        vd.file_type = item.file_type;
                                        vd.created_by = uid;
                                        vd.created_date = DateTime.Now;
                                        vd.created_ip = ip;
                                        vd.created_token_id = tid;
                                        vd.is_active = item.is_active;
                                        vd.group_img = item.group_img;
                                        vd.is_copy = true;
                                        vd.is_order = sttFile;
                                        listAddImagesToDb.Add(vd);
                                    }
                                    sttImg++;
                                }
                                if (listAddImagesToDb.Count > 0)
                                {
                                    db.tivi_screen_image.AddRange(listAddImagesToDb);
                                }
                                List<string> pathImgDel = new List<string>();
                                List<tivi_screen_image> delImg = new List<tivi_screen_image>();
                                if (imageDel.Count() > 0)
                                {
                                    foreach (var imgD in imageDel)
                                    {
                                        var da = db.tivi_screen_image.FirstOrDefault(x => x.tivi_image_id == imgD.tivi_image_id);
                                        if (da != null)
                                        {
                                            if (da.file_path != null)
                                            {
                                                if (db.tivi_screen_image.FirstOrDefault(x => (x.tivi_image_id != imgD.tivi_image_id && x.group_img == da.group_img) || (x.screen_id != screen_data.screen_id && x.file_path == da.file_path)) == null)
                                                {
                                                    pathImgDel.Add(da.file_path);
                                                }
                                            }
                                            delImg.Add(da);
                                        }
                                    }
                                    if (delImg.Count > 0)
                                    {
                                        db.tivi_screen_image.RemoveRange(delImg);
                                    }
                                }

                                //
                                #region log tivi
                                tivi_log logtivi = new tivi_log();
                                logtivi.tivi_id = detail_screen.tivi_id;
                                logtivi.method = "HttpPost";
                                logtivi.organization_id = user_now?.organization_id;
                                logtivi.action = "Tivi/UpdateConfigScreen";
                                logtivi.created_by = uid;
                                logtivi.created_date = DateTime.Now;
                                logtivi.created_ip = ip;
                                logtivi.created_token_id = tid;
                                logtivi.content = JsonConvert.SerializeObject(
                                    new { 
                                        screenEdit = JsonConvert.SerializeObject(detail_screen, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }),
                                        propagandaAdd = JsonConvert.SerializeObject(listAddFilesToDb, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                                        propagandaEdit = JsonConvert.SerializeObject(list_edit_pro, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                                        propagandaDel = JsonConvert.SerializeObject(delData, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                                        imageAdd = JsonConvert.SerializeObject(listAddImagesToDb, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                                        imageEdit = JsonConvert.SerializeObject(list_edit_image, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                                        imageDel = JsonConvert.SerializeObject(delImg, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) 
                                    });
                                db.tivi_log.Add(logtivi);
                                #endregion

                                db.SaveChanges();
                                if (pathImgDel.Count > 0)
                                {
                                    foreach (var pathDel in pathImgDel)
                                    {
                                        var url = Regex.Replace(pathDel.Replace("\\", "/"), @"\.*/+", "/");
                                        var listPath = url.Split('/');
                                        var pathFile = "";
                                        foreach (var item in listPath)
                                        {
                                            if (item.Trim() != "")
                                            {
                                                pathFile += "/" + Path.GetFileName(item);
                                            }
                                        }
                                        if (File.Exists(root + pathFile))
                                        {
                                            File.Delete(root + pathFile);
                                        }
                                    }
                                }
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "2", ms = "Màn hình được chọn không tồn tại." });
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "2", ms = "Màn hình được chọn không tồn tại." });
                        }
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/UpdateConfigScreen", ip, tid, "Lỗi khi cập nhật config chi tiết màn hình TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/UpdateConfigScreen", ip, tid, "Lỗi khi cập nhật config chi tiết màn hình TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> UploadFileImageScreen()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    var user_now = db.sys_users.Find(uid);
                    var organization_id_user = "other";
                    if (user_now != null && user_now.organization_id != null)
                    {
                        organization_id_user = user_now.organization_id.ToString();
                    }
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        var screen_id_up = provider.FormData.GetValues("screen_id").SingleOrDefault();
                        if (screen_id_up != null && screen_id_up.Trim() != "")
                        {
                            // This illustrates how to get thefile names.
                            FileInfo fileInfo = null;
                            MultipartFileData ffileData = null;
                            string newFileName = "";
                            var screenTV = db.tivi_screen_detail.FirstOrDefault(x => x.screen_id == screen_id_up);
                            if (screenTV != null)
                            {
                                screenTV.display_screen_image = true;
                                var numfileImg = db.tivi_screen_image.Count(x => x.screen_id == screen_id_up);
                                List<tivi_screen_image> listFileUp = new List<tivi_screen_image>();
                                List<string> listPathFileUp = new List<string>();
                                foreach (MultipartFileData fileData in provider.FileData)
                                {
                                    numfileImg++;
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
                                    newFileName = Path.Combine("/Portals/" + organization_id_user + "/ConfigTivi", fileName);
                                    var fileNameTemp = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/");
                                    var listPathTemp = fileNameTemp.Split('/');
                                    var pathConfigTemp = "";
                                    foreach (var item in listPathTemp)
                                    {
                                        if (item.Trim() != "")
                                        {
                                            pathConfigTemp += "/" + Path.GetFileName(item);
                                        }
                                    }
                                    newFileName = pathConfigTemp;
                                    fileInfo = new FileInfo(root + newFileName);
                                    Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                    if (fileInfo.Exists)
                                    {
                                        fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                        fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;
                                        // Convert to unsign                                
                                        fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");
                                    }
                                    else
                                    {
                                        fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), ""); ;
                                    }
                                    newFileName = Path.Combine("/Portals/" + organization_id_user + "/ConfigTivi", fileName);
                                    var fileNameTemp_1 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/");
                                    var listPathTemp_1 = fileNameTemp_1.Split('/');
                                    var pathConfigTemp_1 = "";
                                    foreach (var item in listPathTemp_1)
                                    {
                                        if (item.Trim() != "")
                                        {
                                            pathConfigTemp_1 += "/" + Path.GetFileName(item);
                                        }
                                    }
                                    newFileName = pathConfigTemp_1;
                                    tivi_screen_image file_img = new tivi_screen_image();
                                    file_img.tivi_image_id = helper.GenKey();
                                    file_img.file_path = "/Portals/" + organization_id_user + "/ConfigTivi/" + fileName;
                                    file_img.file_name = fileData.Headers.ContentDisposition.FileName.Trim('"');
                                    file_img.file_size = new FileInfo(fileData.LocalFileName).Length;
                                    file_img.file_type = file_img.file_name.Substring(file_img.file_name.LastIndexOf(".") + 1).ToLower();
                                    file_img.is_order = numfileImg;
                                    file_img.created_by = uid;
                                    file_img.created_date = DateTime.Now;
                                    file_img.created_ip = ip;
                                    file_img.created_token_id = tid;
                                    file_img.is_active = true;
                                    file_img.screen_id = screen_id_up;
                                    file_img.organization_id = user_now.organization_id;
                                    file_img.is_copy = false;
                                    listFileUp.Add(file_img);
                                    ffileData = fileData;
                                    //Add file
                                    if (fileInfo != null)
                                    {
                                        var strDirectory = "/Portals/" + organization_id_user + "/ConfigTivi/";
                                        var listPathEdit = Regex.Replace(strDirectory.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                        var pathEdit = "";
                                        foreach (var itemEdit in listPathEdit)
                                        {
                                            if (itemEdit.Trim() != "")
                                            {
                                                pathEdit += "/" + Path.GetFileName(itemEdit);
                                            }
                                        }
                                        if (!Directory.Exists(root + pathEdit))
                                        {
                                            Directory.CreateDirectory(root + pathEdit);
                                        }
                                        //if (!Directory.Exists(fileInfo.Directory.FullName))
                                        //{
                                        //    Directory.CreateDirectory(fileInfo.Directory.FullName);
                                        //}

                                        var listPathEdit_1 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                        var pathEdit_1 = "";
                                        foreach (var itemEdit in listPathEdit_1)
                                        {
                                            if (itemEdit.Trim() != "")
                                            {
                                                pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                            }
                                        }
                                        File.Move(ffileData.LocalFileName, root + pathEdit_1);
                                        //File.Move(ffileData.LocalFileName, newFileName);
                                        helper.ResizeImage(root + pathEdit_1, 3840, 2160, 90); // 4K
                                        //helper.ResizeImage(newFileName, 3840, 2160, 90); // 4K
                                        listPathFileUp.Add(ffileData.LocalFileName);
                                    }
                                }
                                if (listFileUp.Count > 0)
                                {
                                    db.tivi_screen_image.AddRange(listFileUp);
                                }

                                #region log tivi
                                tivi_log logtivi = new tivi_log();
                                logtivi.tivi_id = screenTV.tivi_id;
                                logtivi.method = "HttpPost";
                                logtivi.organization_id = user_now?.organization_id;
                                logtivi.action = "Tivi/UploadFileImageScreen";
                                logtivi.created_by = uid;
                                logtivi.created_date = DateTime.Now;
                                logtivi.created_ip = ip;
                                logtivi.created_token_id = tid;
                                logtivi.content = JsonConvert.SerializeObject(new { imageAdd = JsonConvert.SerializeObject(listFileUp, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) });
                                db.tivi_log.Add(logtivi);
                                #endregion

                                db.SaveChanges();
                                if (listPathFileUp.Count > 0)
                                {
                                    foreach (var path in listPathFileUp)
                                    {
                                        if (System.IO.File.Exists(path))
                                        {
                                            System.IO.File.Delete(path);
                                        }
                                    }
                                }
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "2", ms = "Màn hình được chọn không tồn tại." });
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "2", ms = "Màn hình được chọn không tồn tại." });
                        }
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/UploadFileImageScreen", ip, tid, "Lỗi khi upload file hình ảnh màn hình TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/UploadFileImageScreen", ip, tid, "Lỗi khi upload file hình ảnh màn hình TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> CopyScreenToTivi()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                    string root = HttpContext.Current.Server.MapPath("~/");
                    var provider = new MultipartFormDataStreamProvider(root + "/Portals");
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        var dataConfig = provider.FormData.GetValues("modelTivi").SingleOrDefault();
                        tivi_screen tv_data = JsonConvert.DeserializeObject<tivi_screen>(dataConfig);
                        if (tv_data != null)
                        {
                            var detail_tv = db.tivi_screen.FirstOrDefault(x => x.tivi_id == tv_data.tivi_id);
                            if (detail_tv != null)
                            {
                                List<tivi_screen_detail> list_edit_screen = new List<tivi_screen_detail>();

                                // Xóa màn hình bị remove từ tivi hiện tại
                                var listScreen = provider.FormData.GetValues("listScreenTivi").SingleOrDefault();
                                List<tivi_screen_detail> screen = JsonConvert.DeserializeObject<List<tivi_screen_detail>>(listScreen);
                                var screenTemp = db.tivi_screen_detail.AsNoTracking().Where(x => x.tivi_id == detail_tv.tivi_id).ToList();
                                var screenDel = screenTemp.Where(x => screen.Count(y => y.screen_id == x.screen_id) == 0).ToList();
                                var sttScreen = 1;
                                foreach (var item in screen)
                                {
                                    var existData = db.tivi_screen_detail.FirstOrDefault(x => x.screen_id == item.screen_id);
                                    if (existData != null)
                                    {
                                        list_edit_screen.Add(existData);
                                        existData.is_order = sttScreen;
                                        existData.is_active = item.is_active;
                                    }
                                    sttScreen++;
                                }
                                List<tivi_screen_detail> delScreen = new List<tivi_screen_detail>();
                                if (screenDel.Count() > 0)
                                {
                                    foreach (var scrD in screenDel)
                                    {
                                        var da = db.tivi_screen_detail.FirstOrDefault(x => x.screen_id == scrD.screen_id);
                                        if (da != null)
                                        {
                                            delScreen.Add(da);
                                            if (helper.wlog)
                                            {
                                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { da.screen_id, da.screen_name, da.tivi_id, da.display_screen_calendar, da.display_screen_docs, da.display_screen_shows, da.display_screen_image, da.display_calendar_meeting, da.display_calendar_working, da.display_calendar_duty, da.number_docs, da.calendar_department_id, da.is_change_shows, da.time_change_shows, da.is_change_images, da.time_change_images, da.is_order, da.is_active }), domainurl + "Tivi/CopyScreenToTivi", ip, tid, "Xóa màn hình trong khi copy màn hình tivi", 1, "Tivi");
                                            }
                                        }
                                    }
                                    if (delScreen.Count > 0)
                                    {
                                        db.tivi_screen_detail.RemoveRange(delScreen);
                                    }
                                }
                                // Copy màn hình từ tivi khác
                                var dataTV_Copy = provider.FormData.GetValues("listScreen").SingleOrDefault();
                                List<tivi_screen_detail> listScreen_Copy = JsonConvert.DeserializeObject<List<tivi_screen_detail>>(dataTV_Copy);
                                List<tivi_screen_detail> listScreen_Add = new List<tivi_screen_detail>();
                                List<tivi_screen_propaganda> listPropaganda_Add = new List<tivi_screen_propaganda>();
                                List<tivi_screen_image> listImage_Add = new List<tivi_screen_image>();
                                foreach (var item in listScreen_Copy)
                                {
                                    var screen_id_new = helper.GenKey();
                                    if (item.display_screen_shows == true)
                                    {
                                        var listPropaganda_Copy = db.tivi_screen_propaganda.AsNoTracking().Where(x => x.screen_id == item.screen_id).ToList();
                                        if (listPropaganda_Copy.Count > 0)
                                        {
                                            foreach (var pro in listPropaganda_Copy)
                                            {
                                                pro.tivi_propaganda_id = helper.GenKey();
                                                pro.screen_id = screen_id_new;
                                                pro.created_by = uid;
                                                pro.created_date = DateTime.Now;
                                                pro.created_ip = ip;
                                                pro.created_token_id = tid;
                                                listPropaganda_Add.Add(pro);
                                            }
                                        }
                                    }
                                    if (item.display_screen_image == true)
                                    {
                                        var listImage_Copy = db.tivi_screen_image.AsNoTracking().Where(x => x.screen_id == item.screen_id).ToList();
                                        if (listImage_Copy.Count > 0)
                                        {
                                            foreach (var img in listImage_Copy)
                                            {
                                                img.tivi_image_id = helper.GenKey();
                                                img.screen_id = screen_id_new;
                                                img.created_by = uid;
                                                img.created_date = DateTime.Now;
                                                img.created_ip = ip;
                                                img.created_token_id = tid;
                                                img.is_copy = true;
                                                img.group_img = img.group_img != null ? img.group_img : img.tivi_image_id;
                                                listImage_Add.Add(img);
                                            }
                                        }
                                    }
                                    item.screen_id = screen_id_new;
                                    item.tivi_id = detail_tv.tivi_id;
                                    item.is_order = screen.Count() + 1;
                                    listScreen_Add.Add(item);
                                }
                                if (listScreen_Add.Count > 0)
                                {
                                    db.tivi_screen_detail.AddRange(listScreen_Add);
                                    db.SaveChanges();
                                }
                                if (listPropaganda_Add.Count > 0)
                                {
                                    db.tivi_screen_propaganda.AddRange(listPropaganda_Add);
                                }
                                if (listImage_Add.Count > 0)
                                {
                                    db.tivi_screen_image.AddRange(listImage_Add);
                                }

                                #region log tivi
                                tivi_log logtivi = new tivi_log();
                                logtivi.tivi_id = tv_data.tivi_id;
                                logtivi.method = "HttpPost";
                                logtivi.organization_id = user_now?.organization_id;
                                logtivi.action = "Tivi/CopyScreenToTivi";
                                logtivi.created_by = uid;
                                logtivi.created_date = DateTime.Now;
                                logtivi.created_ip = ip;
                                logtivi.created_token_id = tid;
                                logtivi.content = JsonConvert.SerializeObject(
                                    new { 
                                        screenCopy = JsonConvert.SerializeObject(listScreen_Add, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                                        screenEdit = JsonConvert.SerializeObject(list_edit_screen, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), 
                                        screenDel = JsonConvert.SerializeObject(delScreen, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) 
                                    });
                                db.tivi_log.Add(logtivi);
                                #endregion

                                db.SaveChanges();
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "2", ms = "Tivi được chọn không tồn tại." });
                            }
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "2", ms = "Tivi được chọn không tồn tại." });
                        }
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/CopyScreenToTivi", ip, tid, "Lỗi khi copy màn hình TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/CopyScreenToTivi", ip, tid, "Lỗi khi copy màn hình TV", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpDelete]
        public async Task<HttpResponseMessage> Delete_Tivi([System.Web.Mvc.Bind(Include = "")][FromBody] List<string> id)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;

            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var das = await db.tivi_screen.Where(a => id.Contains(a.tivi_id)).ToListAsync();
                    string root = HttpContext.Current.Server.MapPath("~/");
                    List<string> paths = new List<string>();
                    if (das != null)
                    {
                        List<tivi_screen> del = new List<tivi_screen>();
                        List<string> pathFileDel = new List<string>();
                        List<string> pathFolderDel = new List<string>();
                        foreach (var da in das)
                        {
                            del.Add(da);
                            var listScreenTV = db.tivi_screen_detail.AsNoTracking().Where(y => y.tivi_id == da.tivi_id).Select(x => x.screen_id).Distinct().ToList();
                            var listImageDel = db.tivi_screen_image.Where(x => listScreenTV.Contains(x.screen_id)).ToList();
                            if (listImageDel.Count > 0)
                            {
                                foreach (var pathImg in listImageDel)
                                {
                                    if (db.tivi_screen_image.Count(x => x.file_path == pathImg.file_path && !listScreenTV.Contains(x.screen_id)) == 0)
                                    {
                                        pathFileDel.Add(pathImg.file_path);
                                    }
                                }
                            }
                            var listShowDel = db.tivi_screen_propaganda.Where(x => x.type_data == 2 && listScreenTV.Contains(x.screen_id)).ToList();
                            if (listShowDel.Count > 0)
                            {
                                foreach (var pathImg in listShowDel)
                                {
                                    if (db.tivi_screen_propaganda.Count(x => x.type_data == 2 && x.link_folder == pathImg.link_folder && !listScreenTV.Contains(x.screen_id)) == 0)
                                    {
                                        pathFolderDel.Add(pathImg.link_folder);
                                    }
                                }
                            }
                            #region add cms_logs
                            if (helper.wlog)
                            {
                                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { da.tivi_id, da.tivi_name, da.created_date, da.created_ip, da.created_token_id, da.is_active, da.is_order, da.is_change_screen, da.time_change_screen }), domainurl + "Tivi/Delete_Tivi", ip, tid, "Xóa tivi", 1, "Tivi");
                            }
                            #endregion
                        }
                        if (del.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền xóa dữ liệu." });
                        }
                        db.tivi_screen.RemoveRange(del);
                        if (del.Count > 0)
                        {
                            if (pathFileDel.Count > 0)
                            {
                                foreach (var delPath in pathFileDel)
                                {
                                    var listPath = Regex.Replace(delPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                    var pathDel_Edit = "";
                                    foreach (var item in listPath)
                                    {
                                        if (item.Trim() != "")
                                        {
                                            pathDel_Edit += "/" + Path.GetFileName(item);
                                        }
                                    }
                                    if (File.Exists(root + pathDel_Edit))
                                    {
                                        File.Delete(root + pathDel_Edit);
                                    }
                                }
                            }
                            if (pathFolderDel.Count > 0)
                            {
                                foreach (var delPathFolder in pathFolderDel)
                                {
                                    var listPathFolder = Regex.Replace(delPathFolder.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                    var pathDelFolder_Edit = "";
                                    foreach (var item in listPathFolder)
                                    {
                                        if (item.Trim() != "")
                                        {
                                            pathDelFolder_Edit += "/" + Path.GetFileName(item);
                                        }
                                    }
                                    if (Directory.Exists(root + pathDelFolder_Edit))
                                    {
                                        Directory.Delete(root + pathDelFolder_Edit, true);
                                    }
                                }
                            }
                        }

                        #region log tivi
                        var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                        tivi_log logtivi = new tivi_log();
                        logtivi.tivi_id = das[0].tivi_id;
                        logtivi.method = "HttpPost";
                        logtivi.organization_id = user_now?.organization_id;
                        logtivi.action = "Tivi/Delete_Tivi";
                        logtivi.created_by = uid;
                        logtivi.created_date = DateTime.Now;
                        logtivi.created_ip = ip;
                        logtivi.created_token_id = tid;
                        logtivi.content = JsonConvert.SerializeObject(new { listTiviDel = JsonConvert.SerializeObject(das, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) });
                        db.tivi_log.Add(logtivi);
                        #endregion

                    }
                    await db.SaveChangesAsync();

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Tivi/Delete_Tivi", ip, tid, "Lỗi khi xoá tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = id, contents }), domainurl + "Tivi/Delete_Tivi", ip, tid, "Lỗi khi xoá tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> Tivi_SettingPublic([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string str = data["str"].ToObject<string>();
            string des = Codec.DecryptString(str, helper.psKey);
            JObject jobject = JsonConvert.DeserializeObject<JObject>(des);
            string token = jobject["token"].ToObject<string>();
            settings config = JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @".\Config\Config.json", Encoding.UTF8));
            if (string.IsNullOrEmpty(token) || token != config.publictoken)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập dữ liệu này!", err = "1" });
            }

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = "administrator";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                var user_id = jobject["user_id"] != null ? jobject["user_id"].ToObject<string>() : null;
                var tivi_id = jobject["idtivi"] != null ? jobject["idtivi"].ToObject<string>() : null; // edit
                var sqlpas = new List<SqlParameter>();
                sqlpas.Add(new SqlParameter("@" + "user_id", user_id));
                sqlpas.Add(new SqlParameter("@" + "tivi_id", tivi_id)); // edit
                sqlpas.Add(new SqlParameter("@" + "socket_url", config.socketUrl));
                var arrpas = sqlpas.ToArray();
                //var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "tivi_get_setting_public", arrpas).Tables);
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "tivi_get_config_public", arrpas).Tables); // edit
                var tables = await task;
                string JSONresult = Codec.EncryptString(JsonConvert.SerializeObject(tables), ConfigurationManager.AppSettings["EncriptKey"]);

                #region log tivi
                using (DBEntities db = new DBEntities())
                {
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == user_id);
                    tivi_log logtivi = new tivi_log();
                    logtivi.tivi_id = tivi_id;
                    logtivi.method = "HttpPost";
                    logtivi.organization_id = user_now?.organization_id;
                    logtivi.action = "Tivi/Tivi_SettingPublic";
                    logtivi.created_by = user_id;
                    logtivi.created_date = DateTime.Now;
                    logtivi.created_ip = ip;
                    logtivi.created_token_id = tid;
                    logtivi.content = JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(tables) });
                    db.tivi_log.Add(logtivi);
                    db.SaveChanges();
                }
                #endregion

                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Tivi_SettingPublic", ip, tid, "Lỗi khi lấy dữ liệu setting tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Tivi_SettingPublic", ip, tid, "Lỗi khi lấy dữ liệu setting tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> Tivi_LawPublic([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string str = data["str"].ToObject<string>();
            string des = Codec.DecryptString(str, helper.psKey);
            JObject jobject = JsonConvert.DeserializeObject<JObject>(des);
            string token = jobject["token"].ToObject<string>();
            settings config = JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @".\Config\Config.json", Encoding.UTF8));
            if (string.IsNullOrEmpty(token) || token != config.publictoken)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập dữ liệu này!", err = "1" });
            }

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = "administrator";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                var user_id = jobject["user_id"] != null ? jobject["user_id"].ToObject<string>() : null;
                var tivi_id = jobject["idtivi"] != null ? jobject["idtivi"].ToObject<string>() : null;

                var sqlpas = new List<SqlParameter>();
                sqlpas.Add(new SqlParameter("@" + "user_id", uid));
                var arrpas = sqlpas.ToArray();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "tivi_get_law_screen", arrpas).Tables);
                var tables = await task;
                string JSONresult = Codec.EncryptString(JsonConvert.SerializeObject(tables), ConfigurationManager.AppSettings["EncriptKey"]);

                #region log tivi
                using (DBEntities db = new DBEntities())
                {
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == user_id);
                    tivi_log logtivi = new tivi_log();
                    logtivi.tivi_id = tivi_id;
                    logtivi.method = "HttpPost";
                    logtivi.organization_id = user_now?.organization_id;
                    logtivi.action = "Tivi/Tivi_LawPublic";
                    logtivi.created_by = user_id;
                    logtivi.created_date = DateTime.Now;
                    logtivi.created_ip = ip;
                    logtivi.created_token_id = tid;
                    logtivi.content = JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(tables) });
                    db.tivi_log.Add(logtivi);
                    db.SaveChanges();
                }
                #endregion

                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Tivi_LawPublic", ip, tid, "Lỗi khi lấy dữ liệu văn bản luật tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Tivi_LawPublic", ip, tid, "Lỗi khi lấy dữ liệu văn bản luật tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> Tivi_MediaPublic([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string str = data["str"].ToObject<string>();
            string des = Codec.DecryptString(str, helper.psKey);
            JObject jobject = JsonConvert.DeserializeObject<JObject>(des);
            string token = jobject["token"].ToObject<string>();
            settings config = JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @".\Config\Config.json", Encoding.UTF8));
            if (string.IsNullOrEmpty(token) || token != config.publictoken)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập dữ liệu này!", err = "1" });
            }

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = "administrator";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                var user_id = jobject["user_id"] != null ? jobject["user_id"].ToObject<string>() : null;
                var tivi_id = jobject["idtivi"] != null ? jobject["idtivi"].ToObject<string>() : null;
                var sqlpas = new List<SqlParameter>();
                sqlpas.Add(new SqlParameter("@" + "user_id", uid));
                sqlpas.Add(new SqlParameter("@" + "tivi_id", tivi_id));
                var arrpas = sqlpas.ToArray();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "tivi_get_media_screen", arrpas).Tables);
                var tables = await task;
                string JSONresult = Codec.EncryptString(JsonConvert.SerializeObject(tables), ConfigurationManager.AppSettings["EncriptKey"]);

                #region log tivi
                using (DBEntities db = new DBEntities())
                {
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == user_id);
                    tivi_log logtivi = new tivi_log();
                    logtivi.tivi_id = tivi_id;
                    logtivi.method = "HttpPost";
                    logtivi.organization_id = user_now?.organization_id;
                    logtivi.action = "Tivi/Tivi_MediaPublic";
                    logtivi.created_by = user_id;
                    logtivi.created_date = DateTime.Now;
                    logtivi.created_ip = ip;
                    logtivi.created_token_id = tid;
                    logtivi.content = JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(tables) });
                    db.tivi_log.Add(logtivi);
                    db.SaveChanges();
                }
                #endregion

                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Tivi_MediaPublic", ip, tid, "Lỗi khi lấy dữ liệu video/audio tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Tivi_MediaPublic", ip, tid, "Lỗi khi lấy dữ liệu video/audio tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> Tivi_ImagePublic([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string str = data["str"].ToObject<string>();
            string des = Codec.DecryptString(str, helper.psKey);
            JObject jobject = JsonConvert.DeserializeObject<JObject>(des);
            string token = jobject["token"].ToObject<string>();
            settings config = JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @".\Config\Config.json", Encoding.UTF8));
            if (string.IsNullOrEmpty(token) || token != config.publictoken)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập dữ liệu này!", err = "1" });
            }

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = "administrator";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                var user_id = jobject["user_id"] != null ? jobject["user_id"].ToObject<string>() : null;
                var tivi_id = jobject["idtivi"] != null ? jobject["idtivi"].ToObject<string>() : null;
                var sqlpas = new List<SqlParameter>();
                sqlpas.Add(new SqlParameter("@" + "user_id", uid));
                sqlpas.Add(new SqlParameter("@" + "tivi_id", tivi_id));
                var arrpas = sqlpas.ToArray();
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "tivi_get_image_screen", arrpas).Tables);
                var tables = await task;
                string JSONresult = Codec.EncryptString(JsonConvert.SerializeObject(tables), ConfigurationManager.AppSettings["EncriptKey"]);

                #region log tivi
                using (DBEntities db = new DBEntities())
                {
                    var user_now = db.sys_users.FirstOrDefault(x => x.user_id == user_id);
                    tivi_log logtivi = new tivi_log();
                    logtivi.tivi_id = tivi_id;
                    logtivi.method = "HttpPost";
                    logtivi.organization_id = user_now?.organization_id;
                    logtivi.action = "Tivi/Tivi_ImagePublic";
                    logtivi.created_by = user_id;
                    logtivi.created_date = DateTime.Now;
                    logtivi.created_ip = ip;
                    logtivi.created_token_id = tid;
                    logtivi.content = JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(tables) });
                    db.tivi_log.Add(logtivi);
                    db.SaveChanges();
                }
                #endregion
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Tivi_ImagePublic", ip, tid, "Lỗi khi lấy dữ liệu image tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/Tivi_ImagePublic", ip, tid, "Lỗi khi lấy dữ liệu image tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> List_TiviPublic([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string str = data["str"].ToObject<string>();
            string des = Codec.DecryptString(str, helper.psKey);
            JObject jobject = JsonConvert.DeserializeObject<JObject>(des);
            //string token = jobject["token"].ToObject<string>();
            settings config = JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @".\Config\Config.json", Encoding.UTF8));
            //if (string.IsNullOrEmpty(token) || token != config.publictoken)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập dữ liệu này!", err = "1" });
            //}

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value; ;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                using (DBEntities db = new DBEntities()) 
                {
                    if (uid != null)
                    {
                        var userAccess = db.sys_users.Find(uid);
                        if (userAccess != null && (userAccess.is_admin == true || userAccess.is_super == true))
                        {
                            var user_id = jobject["user_id"] != null ? jobject["user_id"].ToObject<string>() : null;
                            var tivi_id = jobject["idtivi"] != null ? jobject["idtivi"].ToObject<string>() : null;
                            var sqlpas = new List<SqlParameter>();
                            sqlpas.Add(new SqlParameter("@" + "user_id", uid));
                            sqlpas.Add(new SqlParameter("@" + "socket_url", config.socketUrl));
                            var arrpas = sqlpas.ToArray();
                            var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, "tivi_list_public", arrpas).Tables);
                            var tables = await task;
                            string JSONresult = Codec.EncryptString(JsonConvert.SerializeObject(tables), ConfigurationManager.AppSettings["EncriptKey"]);

                            #region log tivi
                            var user_now = db.sys_users.FirstOrDefault(x => x.user_id == user_id);
                            tivi_log logtivi = new tivi_log();
                            logtivi.tivi_id = tivi_id;
                            logtivi.method = "HttpPost";
                            logtivi.organization_id = user_now?.organization_id;
                            logtivi.action = "Tivi/List_TiviPublic";
                            logtivi.created_by = user_id;
                            logtivi.created_date = DateTime.Now;
                            logtivi.created_ip = ip;
                            logtivi.created_token_id = tid;
                            logtivi.content = JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(tables) });
                            db.tivi_log.Add(logtivi);
                            db.SaveChanges();
                            #endregion
                            return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập dữ liệu này!", err = "1" });
                        }
                    }
                    else {
                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập dữ liệu này!", err = "1" });
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/List_TiviPublic", ip, tid, "Lỗi khi lấy danh sách tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/List_TiviPublic", ip, tid, "Lỗi khi lấy danh sách tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<HttpResponseMessage> dangkytivi([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string str = data["str"].ToObject<string>();
            string des = Codec.DecryptString(str, helper.psKey);
            JObject jobject = JsonConvert.DeserializeObject<JObject>(des);
            string token = jobject["token"].ToObject<string>();
            string id_TV = jobject["idtivi"].ToObject<string>();
            string name_TV = jobject["tiviname"].ToObject<string>();
            settings config = JsonConvert.DeserializeObject<settings>(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @".\Config\Config.json", Encoding.UTF8));
            if (string.IsNullOrEmpty(token) || token != config.publictoken)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập dữ liệu này!", err = "1" });
            }

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = "administrator";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var existID_Screen = db.tivi_screen.FirstOrDefault(x => x.tivi_id == id_TV.Trim());
                    var existName_Screen = db.tivi_screen.FirstOrDefault(x => x.tivi_name.ToLower() == name_TV.Trim().ToLower());
                    if (existID_Screen != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "ID Tivi đã được đăng ký trong hệ thống.", err = "1" });
                    }
                    else if (existName_Screen != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Tên Tivi đã được đăng ký trong hệ thống.", err = "1" });
                    }
                    else
                    {
                        var maxOrder = db.tivi_screen.Count() == 0 ? 0 : db.tivi_screen.Max(x => x.is_order);
                        var obj_tv = new tivi_screen();
                        obj_tv.tivi_id = id_TV.Trim();
                        obj_tv.tivi_name = name_TV.Trim();
                        obj_tv.created_date = DateTime.Now;
                        obj_tv.created_ip = ip;
                        obj_tv.created_token_id = tid;
                        obj_tv.is_active = true;
                        obj_tv.is_change_screen = true;
                        obj_tv.time_change_screen = 60;
                        obj_tv.is_order = maxOrder + 1;
                        db.tivi_screen.Add(obj_tv);

                        var user_id = jobject["user_id"] != null ? jobject["user_id"].ToObject<string>() : null;

                        #region log tivi
                        var user_now = db.sys_users.FirstOrDefault(x => x.user_id == user_id);
                        tivi_log logtivi = new tivi_log();
                        logtivi.tivi_id = obj_tv.tivi_id;
                        logtivi.method = "HttpPost";
                        logtivi.organization_id = user_now?.organization_id;
                        logtivi.action = "Tivi/dangkytivi";
                        logtivi.created_by = user_id;
                        logtivi.created_date = DateTime.Now;
                        logtivi.created_ip = ip;
                        logtivi.created_token_id = tid;
                        logtivi.content = JsonConvert.SerializeObject(new { dataTivi = JsonConvert.SerializeObject(obj_tv, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }) });
                        db.tivi_log.Add(logtivi);
                        #endregion

                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/dangkytivi", ip, tid, "Lỗi khi thêm màn hình tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "Tivi/dangkytivi", ip, tid, "Lỗi khi thêm màn hình tivi", 0, "Tivi");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        #region CallProc
        [HttpPost]
        public async Task<HttpResponseMessage> GetDataProc([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);
            string nameErrProc = "Lỗi khi gọi proc";// + proc.proc + "'";

            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
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
                var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables);
                var tables = await task;
                DateTime edate = DateTime.Now;
                #region add SQLLog
                if (helper.wlog)
                {
                    using (DBEntities db = new DBEntities())
                    {
                        sql_log log = new sql_log();
                        log.controller = domainurl + "Tivi/GetDataProc";
                        log.start_date = sdate;
                        log.end_date = edate;
                        log.milliseconds = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
                        log.user_id = uid;
                        log.token_id = tid;
                        log.created_ip = ip;
                        log.created_date = DateTime.Now;
                        log.created_by = uid;
                        log.created_token_id = tid;
                        log.modified_ip = ip;
                        log.modified_date = DateTime.Now;
                        log.modified_by = uid;
                        log.modified_token_id = tid;
                        log.full_name = name;
                        log.title = proc.proc;
                        log.log_content = JsonConvert.SerializeObject(new { data = proc });
                        db.sql_log.Add(log);
                        await db.SaveChangesAsync();
                    }
                }
                #endregion
                string JSONresult = JsonConvert.SerializeObject(tables);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Tivi/GetDataProc", ip, tid, nameErrProc, 0, "Tivi");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Tivi/GetDataProc", ip, tid, nameErrProc, 0, "Tivi");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }
        #endregion

    }
}
