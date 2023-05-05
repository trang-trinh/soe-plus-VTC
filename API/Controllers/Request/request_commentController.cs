using API.Helper;
using API.Models;
using Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.Controllers.Request
{
    [Authorize(Roles = "login")]
    public class request_commentController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        [HttpPost]
        [System.Web.Mvc.ValidateInput(false)]
        public async Task<HttpResponseMessage> Add_Comment()
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
                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        string modelComment = provider.FormData.GetValues("comment").SingleOrDefault();
                        request_comment cmtR = JsonConvert.DeserializeObject<request_comment>(modelComment);
                        List<request_comment> listComment = new List<request_comment>();
                        if (cmtR.request_comment_id == null || cmtR.request_comment_id == "")
                        {
                            var requestNow = db.request_master.Find(cmtR.request_id);
                            if (requestNow != null)
                            {
                                if (cmtR.content != null && cmtR.content.Trim() != "")
                                {
                                    cmtR.request_comment_id = helper.GenKey();
                                    cmtR.is_type = 0;
                                    cmtR.type_comment = 0;
                                    cmtR.created_by = uid;
                                    cmtR.created_date = DateTime.Now;
                                    cmtR.created_ip = ip;
                                    cmtR.created_token_id = tid;
                                    cmtR.is_app = false;
                                    cmtR.is_delete = false;
                                    listComment.Add(cmtR);
                                }
                                string strPath = "/Portals/" + organization_id_user + "/Request/" + requestNow.request_id;

                                var listPathEdit_0 = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit_0 = "";
                                foreach (var itemEdit in listPathEdit_0)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit_0 += "/" + Path.GetFileName(itemEdit);
                                    }
                                }

                                bool exists = Directory.Exists(root + pathEdit_0);
                                if (!exists)
                                    Directory.CreateDirectory(root + pathEdit_0);
                                FileInfo fileInfo = null;
                                MultipartFileData ffileData = null;
                                string newFileName = "";
                                List<request_master_file> listFileUp = new List<request_master_file>();
                                List<string> listPathFileUp = new List<string>();

                                foreach (MultipartFileData fileData in provider.FileData)
                                {
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
                                    newFileName = Path.Combine("/Portals/" + organization_id_user + "/Request/" + requestNow.request_id, Path.GetFileName(fileName));

                                    var listPathEdit_File = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                    var pathEdit_File = "";
                                    foreach (var itemEdit in listPathEdit_File)
                                    {
                                        if (itemEdit.Trim() != "")
                                        {
                                            pathEdit_File += "/" + Path.GetFileName(itemEdit);
                                        }
                                    }

                                    fileInfo = new FileInfo(root + pathEdit_File);
                                    var nameFileOrigin = fileName;
                                    if (fileInfo.Exists)
                                    {
                                        fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                        fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;
                                        // Convert to unsign
                                        Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                        fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                        newFileName = Path.Combine("/Portals/" + organization_id_user + "/Request/" + requestNow.request_id, fileName);
                                    }
                                    string pathFile = "/Portals/" + organization_id_user + "/Request/" + requestNow.request_id + "/" + fileName;
                                    var cmtFile = new request_comment();
                                    cmtFile.request_comment_id = helper.GenKey();
                                    cmtFile.request_id = requestNow.request_id;
                                    cmtFile.is_type = 0;
                                    cmtFile.is_delete = false;
                                    cmtFile.is_app = false;
                                    cmtFile.created_by = uid;
                                    cmtFile.created_date = DateTime.Now;
                                    cmtFile.created_ip = ip;
                                    cmtFile.created_token_id = tid;
                                    cmtFile.parent_id = cmtR.parent_id;
                                    cmtFile.content = null;

                                    if (helper.IsImageFileName(fileName))
                                    {
                                        helper.ResizeImage(domainurl + pathFile, 1920, 1080, 90);
                                        cmtFile.type_comment = 1;
                                    }
                                    else if (helper.IsVideoFileName(fileName))
                                    {
                                        cmtFile.type_comment = 3;
                                    }
                                    else if (helper.IsAudioFileName(fileName))
                                    {
                                        cmtFile.type_comment = 4;
                                    }
                                    else
                                    {
                                        cmtFile.type_comment = 2;
                                    }
                                    listComment.Add(cmtFile);

                                    request_master_file fileCmt = new request_master_file();
                                    fileCmt.request_file_id = helper.GenKey();
                                    fileCmt.request_id = requestNow.request_id;
                                    fileCmt.request_comment_id = cmtFile.request_comment_id;
                                    fileCmt.file_type = helper.GetFileExtension(fileName);
                                    fileCmt.file_name = nameFileOrigin;
                                    fileCmt.file_path = pathFile;
                                    fileCmt.file_size = new FileInfo(fileData.LocalFileName).Length;
                                    fileCmt.is_image = cmtFile.type_comment == 1 ? true : false;
                                    fileCmt.is_type = 1;
                                    fileCmt.is_delete = false;
                                    fileCmt.created_by = uid;
                                    fileCmt.created_date = cmtFile.created_date;
                                    fileCmt.created_ip = ip;
                                    fileCmt.created_token_id = tid;
                                    listFileUp.Add(fileCmt);

                                    ffileData = fileData;
                                    if (fileInfo != null)
                                    {
                                        var strDirectory = "/Portals/" + organization_id_user + "/Request/" + requestNow.request_id;
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
                                        listPathFileUp.Add(ffileData.LocalFileName);
                                    }
                                }
                                if (listComment.Count > 0)
                                {
                                    db.request_comment.AddRange(listComment);
                                    db.SaveChanges();
                                }

                                if (listFileUp.Count > 0)
                                {
                                    db.request_master_file.AddRange(listFileUp);
                                }

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
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Đề xuất không tồn tại." });
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "request_comment/Add_Comment", ip, tid, "Lỗi khi thêm mới comment đề xuất", 0, "request_comment");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "request_comment/Add_Comment", ip, tid, "Lỗi khi thêm mới comment đề xuất", 0, "request_comment");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        // ---
    }
}
