using API.Helper;
using API.Models;
using Helper;
using HtmlAgilityPack;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Controllers
{
    [Authorize(Roles = "login")]
    public class HelpController : ApiController
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
        //get all chill from parent
        List<int> arr_childs = new List<int>();
        public void getChill(int? Parent_ID, List<help_title> Data)
        {
            DBEntities db = new DBEntities();
            Data.ForEach(item =>
            {
                if (item.parent_id == Parent_ID)
                {
                    arr_childs.Add(item.help_title_id);
                    List<help_title> listChild = db.help_title.Where(x => x.parent_id == item.help_title_id).ToList();
                    if (listChild.Count() > 0)
                        getChill(item.help_title_id, listChild);
                }
            });
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Add_Content()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");

                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        var md = provider.FormData.GetValues("model").SingleOrDefault();
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        help_content model = JsonConvert.DeserializeObject<help_content>(fdmodel);
                        #region covert file base 64
                        //string strPath = root + "/" + dvid + "/Help/" + model.help_title_id;
                        //bool exists = Directory.Exists(strPath);
                        //if (!exists)
                        //    Directory.CreateDirectory(strPath);
                        //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        //doc.LoadHtml(model.content);
                        //var imgs = doc.DocumentNode.SelectNodes("//img");
                        //if (imgs != null)
                        //{
                        //    foreach (var img in imgs)
                        //    {
                        //        HtmlNode oldChild = img;
                        //        HtmlNode newChild = HtmlNode.CreateNode(img.OuterHtml);
                        //        var pathFolderDes = "/Portals/" + dvid + "/Help/" + model.help_title_id;
                        //        var checkBase64 = img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7);
                        //        checkBase64 = checkBase64.Trim();
                        //        if ((checkBase64.Length % 4 == 0) && Regex.IsMatch(checkBase64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                        //        {
                        //            byte[] bytes = Convert.FromBase64String(img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7));
                        //            bool existsFolder = System.IO.Directory.Exists(strPath + pathFolderDes);
                        //            if (!existsFolder)
                        //            {
                        //                System.IO.Directory.CreateDirectory(strPath);
                        //            }

                        //            var index1 = img.Attributes["src"].Value.LastIndexOf("data:image/") + 11;
                        //            var index2 = img.Attributes["src"].Value.IndexOf("base64,");
                        //            var typeFileHL = "." + img.Attributes["src"].Value.Substring(index1, index2 - index1 - 1);
                        //            var pathShow = "/" + helper.GenKey() + typeFileHL;



                        //            using (var imageFile = new FileStream(strPath + pathShow, FileMode.Create))
                        //            {
                        //                imageFile.Write(bytes, 0, bytes.Length);
                        //                imageFile.Flush();
                        //            }

                        //            img.SetAttributeValue("style", "width:100%;height:100%;max-height:500px;object-fit:contain");
                        //            img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/Help/" + model.help_title_id + pathShow);
                        //            model.content = model.content.Replace(newChild.OuterHtml, img.OuterHtml);
                        //            helper.ResizeImage(strPath + pathShow, 640, 640, 90);
                        //        }
                        //        else
                        //        {

                        //            bool existsFolder = System.IO.File.Exists(strPath + img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.IndexOf("/Help") + 5));
                        //            if (!existsFolder)
                        //            {
                        //                System.Net.WebClient webc = new System.Net.WebClient();
                        //                var pathShow = "/" + helper.GenKey() + ".png";
                        //                FileStream imageFile1 = null;
                        //                try
                        //                {
                        //                    imageFile1 = new FileStream(strPath + pathShow, FileMode.Create);

                        //                }

                        //                finally
                        //                {
                        //                    imageFile1.Close();
                        //                }

                        //                webc.DownloadFile(img.Attributes["src"].Value, strPath + pathShow);


                        //                img.SetAttributeValue("style", "width:100%;height:100%;max-height:500px;object-fit:contain");
                        //                img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/Help/" + model.help_title_id + pathShow);
                        //                model.content = model.content.Replace(newChild.OuterHtml, img.OuterHtml);
                        //                helper.ResizeImage(strPath + pathShow, 640, 640, 90);

                        //            }
                        //        }
                        //    }
                        //}

                        #endregion

                        // This illustrates how to get thefile names.
                        model.created_date = DateTime.Now;
                        model.created_by = uid;
                        model.created_token_id = tid;
                        model.created_ip = ip;
                        db.help_content.Add(model);
                        db.SaveChanges();
                        #region  add Log
                        // helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents = "" }), domainurl + "Users/Add_Users", ip, tid, "Thêm mới User " + model.user_id, 1, "Users");
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Help/Add_Content", ip, tid, "Lỗi khi thêm nội dung", 0, "Help");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Help/Update_Content", ip, tid, "Lỗi khi thêm nội dung", 0, "Users");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Content()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");

                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        var md = provider.FormData.GetValues("model").SingleOrDefault();
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        help_content model = JsonConvert.DeserializeObject<help_content>(fdmodel);
                        #region convert base 64 
                        //string strPath = root + "/" + dvid + "/Help/" + model.help_title_id;
                        //bool exists = Directory.Exists(strPath);
                        //if (!exists)
                        //    Directory.CreateDirectory(strPath);
                        //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        //doc.LoadHtml(model.content);
                        //// del old file
                        //System.IO.DirectoryInfo di = new DirectoryInfo(strPath);

                        //foreach (FileInfo file in di.GetFiles())
                        //{
                        //    file.Delete();
                        //}
                        //foreach (DirectoryInfo dir in di.GetDirectories())
                        //{
                        //    dir.Delete(true);
                        //}
                        //var imgs = doc.DocumentNode.SelectNodes("//img");
                        //if (imgs != null)
                        //{
                        //    foreach (var img in imgs)
                        //    {
                        //        HtmlNode oldChild = img;
                        //        HtmlNode newChild = HtmlNode.CreateNode(img.OuterHtml);
                        //        var pathFolderDes = "/Portals/" + dvid + "/Help/" + model.help_title_id;
                        //        var checkBase64 = img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7);
                        //        checkBase64 = checkBase64.Trim();
                        //        if ((checkBase64.Length % 4 == 0) && Regex.IsMatch(checkBase64, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                        //        {
                        //            byte[] bytes = Convert.FromBase64String(img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.LastIndexOf("base64,") + 7));
                        //            bool existsFolder = System.IO.Directory.Exists(strPath + pathFolderDes);
                        //            if (!existsFolder)
                        //            {
                        //                System.IO.Directory.CreateDirectory(strPath);
                        //            }

                        //            var index1 = img.Attributes["src"].Value.LastIndexOf("data:image/") + 11;
                        //            var index2 = img.Attributes["src"].Value.IndexOf("base64,");
                        //            var typeFileHL = "." + img.Attributes["src"].Value.Substring(index1, index2 - index1 - 1);
                        //            var pathShow = "/" + helper.GenKey() + typeFileHL;



                        //            using (var imageFile = new FileStream(strPath + pathShow, FileMode.Create))
                        //            {
                        //                imageFile.Write(bytes, 0, bytes.Length);
                        //                imageFile.Flush();
                        //            }



                        //            img.SetAttributeValue("style", "width:100%;height:100%;max-height:500px;object-fit:contain");
                        //            img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/Help/" + model.help_title_id + pathShow);
                        //            model.content = model.content.Replace(newChild.OuterHtml, img.OuterHtml);
                        //            helper.ResizeImage(strPath + pathShow, 640, 640, 90);
                        //        }
                        //        else
                        //        {

                        //            bool existsFolder = System.IO.File.Exists(strPath + img.Attributes["src"].Value.Substring(img.Attributes["src"].Value.IndexOf("/News") + 5));
                        //            if (!existsFolder)
                        //            {
                        //                System.Net.WebClient webc = new System.Net.WebClient();
                        //                var pathShow = "/" + helper.GenKey() + ".png";
                        //                FileStream imageFile1 = null;
                        //                try
                        //                {
                        //                    imageFile1 = new FileStream(strPath + pathShow, FileMode.Create);

                        //                }

                        //                finally
                        //                {
                        //                    imageFile1.Close();
                        //                }

                        //                webc.DownloadFile(img.Attributes["src"].Value, strPath + pathShow);


                        //                img.SetAttributeValue("style", "width:100%;height:100%;max-height:500px;object-fit:contain");
                        //                img.SetAttributeValue("src", domainurl + "/Portals/" + dvid + "/Help/" + model.help_title_id + pathShow);
                        //                model.content = model.content.Replace(newChild.OuterHtml, img.OuterHtml);
                        //                helper.ResizeImage(strPath + pathShow, 640, 640, 90);
                        //            }
                        //        }
                        //    }
                        //}
                        #endregion
                        // This illustrates how to get thefile names.
                        model.modified_date = DateTime.Now;
                        model.modified_by = uid;
                        model.modified_token_id = tid;
                        model.modified_ip = ip;
                        db.Entry(model).State = EntityState.Modified;
                        db.SaveChanges();
                        #region  add Log
                        // helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents = "" }), domainurl + "Users/Add_Users", ip, tid, "Thêm mới User " + model.user_id, 1, "Users");
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Help/Add_Content", ip, tid, "Lỗi khi thêm nội dung", 0, "Help");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Help/Update_Content", ip, tid, "Lỗi khi thêm nội dung", 0, "Users");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Add_Menu()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        var md = provider.FormData.GetValues("model").SingleOrDefault();
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        help_title model = JsonConvert.DeserializeObject<help_title>(fdmodel);
                        // This illustrates how to get thefile names.
                        model.created_date = DateTime.Now;
                        model.created_by = uid;
                        model.created_token_id = tid;
                        model.created_ip = ip;
                        db.help_title.Add(model);
                        db.SaveChanges();
                        #region  add Log
                        // helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents = "" }), domainurl + "Users/Add_Users", ip, tid, "Thêm mới User " + model.user_id, 1, "Users");
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Help/Add_Menu", ip, tid, "Lỗi khi thêm nội dung", 0, "Help");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Help/Update_Menu", ip, tid, "Lỗi khi thêm nội dung", 0, "Users");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Menu()
        {
            var identity = User.Identity as ClaimsIdentity;
            string fdmodel = "";
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        var md = provider.FormData.GetValues("model").SingleOrDefault();
                        fdmodel = provider.FormData.GetValues("model").SingleOrDefault();
                        help_title model = JsonConvert.DeserializeObject<help_title>(fdmodel);
                        // This illustrates how to get thefile names.
                        model.created_date = DateTime.Now;
                        model.created_by = uid;
                        model.created_token_id = tid;
                        model.created_ip = ip;
                        db.Entry(model).State = EntityState.Modified;
                        db.SaveChanges();
                        #region  add Log
                        // helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents = "" }), domainurl + "Users/Add_Users", ip, tid, "Thêm mới User " + model.user_id, 1, "Users");
                        #endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }

            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Help/Add_Content", ip, tid, "Lỗi khi thêm nội dung", 0, "Help");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdmodel, contents }), domainurl + "Help/Update_Content", ip, tid, "Lỗi khi thêm nội dung", 0, "Users");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpDelete]
        public async Task<HttpResponseMessage> Del_Menu([System.Web.Mvc.Bind(Include = "")][FromBody] List<int> ids)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            try
            {
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
                        var del = db.help_title.FirstOrDefault(a => ids.Contains(a.help_title_id));
                        if (del != null)
                        {
                            getChill(del.help_title_id, db.help_title.ToList());
                            List<help_title> dels = new List<help_title>();
                            foreach (var item in arr_childs)
                            {
                                var del_child = db.help_title.FirstOrDefault(x => x.help_title_id == item);
                                if (del_child != null)
                                {
                                    dels.Add(del_child);
                                }
                            }
                            db.help_title.RemoveRange(dels);

                            db.help_title.Remove(del);
                        }
                        await db.SaveChangesAsync();
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Help/Del_Menu", ip, tid, "Lỗi khi xoá menu", 0, "Share");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = ids, contents }), domainurl + "Help/Del_Menu", ip, tid, "Lỗi khi xoá menu", 0, "Share");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
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
                        log.controller = domainurl + "User/GetDataProc";
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
                // return (is_mahoa, key ) string 
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "chat/GetDataProc", ip, tid, "Lỗi khi gọi proc '" + proc + "'", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "chat/GetDataProc", ip, tid, "Lỗi khi gọi proc '" + proc + "'", 0, "chat");
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