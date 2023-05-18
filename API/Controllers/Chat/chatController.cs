using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class chatController : ApiController
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

        #region Chat new
        [HttpPost]
        public async Task<HttpResponseMessage> Add_Chat()
        {
            string modelChat = "";
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
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
                    string nencu = "";
                    string chat_group_id_add = null;
                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        modelChat = provider.FormData.GetValues("models").SingleOrDefault();
                        chat_group chat_main = JsonConvert.DeserializeObject<chat_group>(modelChat);
                        var isAddNew = false;
                        if (chat_main.chat_group_id == null || chat_main.chat_group_id == "")
                        {
                            chat_main.chat_group_id = helper.GenKey();
                            chat_group_id_add = chat_main.chat_group_id;
                            isAddNew = true;
                        }
                        string strPath = root + "/Portals/" + organization_id_user + "/Chat/" + chat_main.chat_group_id + "/Avatar";

                        // format strPath
                        string checkPathFile = Regex.Replace(strPath.Replace("\\", "/"), @"\.*/+", "/");
                        var listPath = checkPathFile.Split('/');
                        var config_strPath = "";
                        var sttPartPathFile = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPathFile == 1)
                                {
                                    config_strPath += (item);
                                }
                                else
                                {
                                    config_strPath += "/" + Path.GetFileName(item);
                                }                                
                            }
                            sttPartPathFile++;
                        }

                        bool exists = Directory.Exists(config_strPath);
                        if (!exists)
                            Directory.CreateDirectory(config_strPath);

                        // This illustrates how to get thefile names.
                        FileInfo fileInfo = null;
                        MultipartFileData ffileData = null;
                        string newFileName = "";
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
                            newFileName = Path.Combine("/Portals/" + organization_id_user + "/Chat/" + chat_main.chat_group_id + "/Avatar", fileName);

                            var listPathEdit_1 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                            var pathEdit_1 = "";
                            foreach (var itemEdit in listPathEdit_1)
                            {
                                if (itemEdit.Trim() != "")
                                {
                                    pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                }
                            }
                            newFileName = pathEdit_1;

                            fileInfo = new FileInfo(newFileName);
                            if (fileInfo.Exists)
                            {
                                fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                fileName = fileName + helper.ranNumberFile() + fileInfo.Extension;
                                // Convert to unsign
                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                fileName = pattern.Replace(helper.convertToUnSignChar(fileName.Replace("%", "percent"), "_"), "");

                                newFileName = Path.Combine("/Portals/" + organization_id_user + "/Chat/" + chat_main.chat_group_id + "/Avatar", fileName);
                            }

                            chat_main.avatar_group = "/Portals/" + organization_id_user + "/Chat/" + chat_main.chat_group_id + "/Avatar/" + fileName;

                            ffileData = fileData;
                            //Add file
                            if (fileInfo != null)
                            {
                                var strDirectory = "/Portals/" + organization_id_user + "/Chat/" + chat_main.chat_group_id + "/Avatar";
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

                                var listPathEdit_2 = Regex.Replace(newFileName.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                var pathEdit_2 = "";
                                foreach (var itemEdit in listPathEdit_2)
                                {
                                    if (itemEdit.Trim() != "")
                                    {
                                        pathEdit_2 += "/" + Path.GetFileName(itemEdit);
                                    }
                                }
                                File.Move(ffileData.LocalFileName, root + pathEdit_2);
                                //File.Move(ffileData.LocalFileName, newFileName);
                                helper.ResizeImage(newFileName, 1920, 1080, 90);
                                listPathFileUp.Add(ffileData.LocalFileName);
                            }
                        }

                        string typeChangeChat = "";
                        if (isAddNew == true)
                        {
                            chat_main.created_by = uid;
                            chat_main.created_date = DateTime.Now;
                            chat_main.created_ip = ip;
                            chat_main.created_token_id = tid;
                            if (chat_main.organization_id == null && organization_id_user != "other")
                            {
                                chat_main.organization_id = Int32.Parse(organization_id_user);
                            }
                            // db.chat_group.Add(chat_main);
                            // add member
                            chat_member mem = new chat_member();
                            mem.chat_member_id = helper.GenKey();
                            mem.chat_group_id = chat_main.chat_group_id;
                            mem.user_join = uid;
                            mem.date_join = DateTime.Now;
                            mem.date_out = null;
                            mem.status = 1;
                            mem.number_not_seen = 0;
                            mem.is_captain = true;
                            mem.is_notify = true;
                            db.chat_member.Add(mem);

                            if (chat_main.user_chat != null && chat_main.is_group_chat != true)
                            {
                                chat_member mem_chat = new chat_member();
                                mem_chat.chat_member_id = helper.GenKey();
                                mem_chat.chat_group_id = chat_main.chat_group_id;
                                mem_chat.user_join = chat_main.user_chat;
                                mem_chat.date_join = DateTime.Now;
                                mem_chat.date_out = null;
                                mem_chat.status = 1;
                                mem_chat.number_not_seen = 0;
                                mem_chat.is_captain = false;
                                mem_chat.is_notify = true;
                                db.chat_member.Add(mem_chat);
                            }
                        }
                        else
                        {
                            var po = db.chat_group.AsNoTracking().FirstOrDefault(a => a.chat_group_id == chat_main.chat_group_id);
                            nencu = po.avatar_group;
                            if (po.is_group_chat == true)
                            {
                                var isChangeAvtOrName = false;
                                typeChangeChat += (user_now.full_name + " đã cập nhật ");
                                if ((chat_main.avatar_group != null && po.avatar_group == null) || (chat_main.avatar_group == null && po.avatar_group != null))
                                {
                                    isChangeAvtOrName = true;
                                    typeChangeChat += "ảnh đại diện";
                                }
                                if (chat_main.chat_group_name != po.chat_group_name)
                                {
                                    typeChangeChat += isChangeAvtOrName == true ? ", tên" : "tên";
                                    isChangeAvtOrName = true;
                                }
                                typeChangeChat += " cuộc trò chuyện " + chat_main.chat_group_name;
                                if (isChangeAvtOrName == false)
                                {
                                    typeChangeChat = "";
                                }
                            }

                            chat_main.modified_by = uid;
                            chat_main.modified_date = DateTime.Now;
                            chat_main.modified_ip = ip;
                            chat_main.modified_token_id = tid;
                            db.Entry(chat_main).State = EntityState.Modified;
                        }

                        string data_member = provider.FormData.GetValues("members").SingleOrDefault();
                        List<chat_member> listMember = JsonConvert.DeserializeObject<List<chat_member>>(data_member);
                        string data_mess = provider.FormData.GetValues("messages").SingleOrDefault();
                        chat_message ms = JsonConvert.DeserializeObject<chat_message>(data_mess);
                        List<string> idMembers = new List<string>();
                        List<string> idMembersGetChange = new List<string>();
                        string pushname = "";
                        if (listMember.Count > 0)
                        {
                            List<chat_member> listMemberAdd = new List<chat_member>();
                            List<chat_member> listMemberDel = new List<chat_member>();
                            foreach (var p in listMember)
                            {
                                if (p.chat_member_id == null)
                                {
                                    idMembers.Add(p.user_join);
                                    pushname += db.sys_users.Find(p.user_join).full_name + ", ";

                                    p.chat_member_id = helper.GenKey();
                                    p.chat_group_id = chat_main.chat_group_id;
                                    p.date_join = DateTime.Now;
                                    p.date_out = null;
                                    p.status = 1;
                                    p.number_not_seen = 0;
                                    p.is_captain = false;
                                    p.is_notify = true;
                                    listMemberAdd.Add(p);
                                }
                                if (typeChangeChat != "")
                                {
                                    idMembersGetChange.Add(p.user_join);
                                }
                            }
                            if (listMemberAdd.Count > 0)
                            {
                                db.chat_member.AddRange(listMemberAdd);
                            }
                        }
                        if (idMembers.Count > 0)
                        {
                            if (ms.chat_message_id == "-1" || ms.chat_message_id == "" || ms.chat_message_id == null)
                            {
                                pushname = pushname.Substring(0, pushname.Length - 2);
                                ms.chat_message_id = helper.GenKey();
                                ms.chat_group_id = chat_main.chat_group_id;
                                ms.content_message = user_now.full_name + " vừa thêm " + pushname + " vào nhóm.";
                                ms.created_by = uid;
                                ms.created_date = DateTime.Now;
                                ms.created_ip = ip;
                                ms.created_token_id = tid;
                                ms.type_message = 5;
                                ms.status = 1;
                                chat_main.modified_date = ms.created_date;
                                db.chat_message.Add(ms);
                            }
                            else
                            {
                                db.Entry(ms).State = EntityState.Modified;
                            }
                        }
                        if (isAddNew == true)
                        {
                            db.chat_group.Add(chat_main);
                            #region add chat_logs
                            if (helper.wlog)
                            {
                                chat_logs log = new chat_logs();
                                log.log_type = 0;
                                log.message = "Thêm mới cuộc trò chuyện: " + chat_main.chat_group_name;
                                log.chat_group_id = chat_main.chat_group_id;
                                log.organization_id = chat_main.organization_id;
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                log.is_view = false;
                                db.chat_logs.Add(log);
                                //db.SaveChanges();
                            }
                            #endregion
                        }
                        else
                        {
                            #region modify chat_logs
                            if (helper.wlog)
                            {
                                chat_logs log = new chat_logs();
                                log.log_type = 0;
                                log.message = "Chỉnh sửa cuộc trò chuyện: " + chat_main.chat_group_name;
                                log.chat_group_id = chat_main.chat_group_id;
                                log.organization_id = chat_main.organization_id;
                                log.created_date = DateTime.Now;
                                log.created_by = uid;
                                log.created_token_id = tid;
                                log.created_ip = ip;
                                log.is_view = false;
                                db.chat_logs.Add(log);
                            }
                            #endregion
                        }
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

                        if (nencu != null && nencu.Trim() != "" && nencu != chat_main.avatar_group)
                        {
                            var listPathEdit = Regex.Replace(nencu.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                            var pathEdit = "";
                            foreach (var itemEdit in listPathEdit)
                            {
                                if (itemEdit.Trim() != "")
                                {
                                    pathEdit += "/" + Path.GetFileName(itemEdit);
                                }
                            }
                            if (System.IO.File.Exists(root + pathEdit))
                            {
                                System.IO.File.Delete(root + pathEdit);
                            }
                        }
                        db.SaveChanges();
                        #region SendNoti
                        List<string> tokens = new List<string>();
                        //string ConnectionSQL = db.Database.Connection.ConnectionString;
                        string ConnectionSQL = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                        var sqlpas = new List<SqlParameter>();
                        sqlpas.Add(new SqlParameter("@" + "chat_group_id", chat_main.chat_group_id));
                        sqlpas.Add(new SqlParameter("@" + "users_ID", uid));
                        var arrpas = sqlpas.ToArray();
                        var task = SqlHelper.ExecuteDataset(ConnectionSQL, "chat_member_token", arrpas).Tables[0];
                        //var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(ConnectionSQL, "chat_member_token", arrpas).Tables[0]);
                        var tables = task;

                        List<UserToken> dataMember = UserTokenByTable(tables);
                        if (idMembers.Count > 0)
                        {
                            var content = "<soe>" + user_now.full_name + "</soe> vừa thêm <soe>" + pushname + " vào nhóm chat: <soe>" + chat_main.chat_group_name + "</soe>";
                            List<string> sendUsers = dataMember.Select(x => x.users_id).ToList();
                            helper.send_noti_chat(user_now.user_id, chat_main.chat_group_id, chat_main.chat_group_id, sendUsers, "Tin nhắn", content, 0, ip, tid);
                        }

                        if (idMembersGetChange.Count > 0)
                        {
                            List<string> sendUsers = dataMember.Select(x => x.users_id).ToList();
                            helper.send_noti_chat(user_now.user_id, chat_main.chat_group_id, chat_main.chat_group_id, sendUsers, "Tin nhắn", typeChangeChat, 0, ip, tid);
                        }
                        #endregion Sendnoti
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", chatGroupID = chat_group_id_add });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Add_Chat", ip, tid, "Lỗi khi cập nhật cuộc hội thoại", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Add_Chat", ip, tid, "Lỗi khi cập nhật cuộc hội thoại", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        [System.Web.Mvc.ValidateInput(false)]
        public async Task<HttpResponseMessage> Add_Message()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
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
                    string root = HttpContext.Current.Server.MapPath("~/Portals");
                    string jwtcookie = HttpContext.Current.Request.Cookies != null && HttpContext.Current.Request.Cookies["jwt"] != null ? HttpContext.Current.Request.Cookies["jwt"].Value : null;
                    var provider = new MultipartFormDataStreamProvider(root);
                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        string modelMessage = provider.FormData.GetValues("models").SingleOrDefault();
                        chat_message message = JsonConvert.DeserializeObject<chat_message>(modelMessage);
                        List<chat_message> listMessage = new List<chat_message>();
                        List<chat_message> listMesReturn = new List<chat_message>();
                        if (message.chat_message_id == null || message.chat_message_id == "")
                        {
                            var chatGroupNow = db.chat_group.Find(message.chat_group_id);
                            if (chatGroupNow != null)
                            {
                                var memberChatNow = db.chat_member.FirstOrDefault(x => x.chat_group_id == message.chat_group_id && x.user_join == uid);
                                string id_member = null;
                                id_member = memberChatNow != null ? memberChatNow.chat_member_id : null;
                                if (message.content_message != null && message.content_message != "")
                                {
                                    message.chat_message_id = helper.GenKey();
                                    message.type_message = 0;
                                    message.chat_member_id = id_member;
                                    message.created_by = uid;
                                    message.created_date = DateTime.Now;
                                    message.created_ip = ip;
                                    message.created_token_id = tid;
                                    memberChatNow.last_message_id = message.chat_message_id;
                                    memberChatNow.last_seen_date = DateTime.Now;
                                    memberChatNow.number_not_seen = 0;
                                    chatGroupNow.modified_date = message.created_date;
                                    if (message.status == null)
                                    {
                                        message.status = 0;
                                    }
                                    listMessage.Add(message);
                                    listMesReturn.Add(new chat_message()
                                    {
                                        chat_message_id = message.chat_message_id,
                                        chat_parent_id = message.chat_parent_id,
                                        type_message = message.type_message,
                                    });
                                }
                                string strPath = "/" + organization_id_user + "/Chat/" + chatGroupNow.chat_group_id;

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
                                List<chat_file> listFileUp = new List<chat_file>();
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
                                    newFileName = Path.Combine("/" + organization_id_user + "/Chat/" + chatGroupNow.chat_group_id, Path.GetFileName(fileName));

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

                                        newFileName = Path.Combine("/" + organization_id_user + "/Chat/" + chatGroupNow.chat_group_id, fileName);
                                    }
                                    string pathFile = "/Portals/" + organization_id_user + "/Chat/" + chatGroupNow.chat_group_id + "/" + fileName;
                                    var mesFile = new chat_message();
                                    mesFile.chat_message_id = helper.GenKey();
                                    mesFile.chat_group_id = chatGroupNow.chat_group_id;
                                    mesFile.chat_member_id = id_member;
                                    mesFile.created_by = uid;
                                    mesFile.created_date = DateTime.Now;
                                    mesFile.created_ip = ip;
                                    mesFile.created_token_id = tid;
                                    mesFile.chat_parent_id = message.chat_parent_id;
                                    mesFile.content_message = null;
                                    if (mesFile.status == null)
                                    {
                                        mesFile.status = 0;
                                    }
                                    //mesFile.type_message
                                    if (helper.IsImageFileName(fileName))
                                    {
                                        helper.ResizeImage(domainurl + pathFile, 1920, 1080, 90);
                                        mesFile.type_message = 1;
                                    }
                                    else if (helper.IsVideoFileName(fileName))
                                    {
                                        mesFile.type_message = 3;
                                    }
                                    else if (helper.IsAudioFileName(fileName))
                                    {
                                        mesFile.type_message = 4;
                                    }
                                    else
                                    {
                                        mesFile.type_message = 2;
                                    }
                                    chatGroupNow.modified_date = mesFile.created_date;
                                    listMessage.Add(mesFile);
                                    memberChatNow.last_message_id = mesFile.chat_message_id;
                                    memberChatNow.last_seen_date = DateTime.Now;
                                    memberChatNow.number_not_seen = 0;

                                    chat_file fileMes = new chat_file();
                                    fileMes.file_id = helper.GenKey();
                                    fileMes.chat_group_id = chatGroupNow.chat_group_id;
                                    fileMes.chat_message_id = mesFile.chat_message_id;
                                    fileMes.file_type = helper.GetFileExtension(fileName);
                                    fileMes.file_name = nameFileOrigin;
                                    fileMes.file_path = pathFile;
                                    fileMes.file_size = new FileInfo(fileData.LocalFileName).Length;
                                    fileMes.is_image = mesFile.type_message == 1 ? true : false;
                                    fileMes.created_by = uid;
                                    fileMes.created_date = mesFile.created_date;
                                    fileMes.created_ip = ip;
                                    fileMes.created_token_id = tid;
                                    listFileUp.Add(fileMes);
                                    listMesReturn.Add(new chat_message()
                                    {
                                        chat_message_id = mesFile.chat_message_id,
                                        chat_parent_id = mesFile.chat_parent_id,
                                        type_message = mesFile.type_message,
                                    });

                                    ffileData = fileData;
                                    if (fileInfo != null)
                                    {
                                        var strDirectory = "/" + organization_id_user + "/Chat/" + chatGroupNow.chat_group_id;
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
                                        //File.Move(ffileData.LocalFileName, root + pathEdit_1);
                                        //listPathFileUp.Add(ffileData.LocalFileName);
                                        helper.UploadFileToDestination(jwtcookie, root, ffileData, pathEdit_1, 360, 360);
                                        //System.Threading.Tasks.Task.Run(() =>
                                        //{
                                            var Portals = ConfigurationManager.AppSettings["Portals"];
                                            sys_file_mapping fm = new sys_file_mapping();
                                            fm.file_key_id = helper.GenKey();
                                            fm.file_id = fileMes.file_id;
                                            fm.file_path = fileMes.file_path;
                                            fm.file_name = fileMes.file_name;
                                            fm.file_size = fileMes.file_size;
                                            fm.file_title = fileMes.file_name;
                                            fm.file_table = "chat_file";
                                            if (string.IsNullOrWhiteSpace(Portals))
                                            {
                                                fm.type_path = 0;
                                            }
                                            else if (Portals.Contains("ftp"))
                                            {
                                                fm.type_path = 1;
                                            }
                                            else if (Portals.Contains("http"))
                                            {
                                                fm.type_path = 2;
                                            }
                                            fm.module_key = "M8";
                                            fm.role_access = null;
                                            var memberChat = db.chat_member.AsNoTracking().Where(z => z.chat_group_id == chatGroupNow.chat_group_id).Select(c => c.user_join).ToList();
                                            fm.user_access = "";
                                            foreach (var userID in memberChat)
                                            {
                                                fm.user_access += (fm.user_access != "" ? "," : "") + userID;
                                            }
                                            fm.deny_access = null;
                                            fm.created_by = uid;
                                            fm.created_date = fileMes.created_date;
                                            fm.created_ip = ip;
                                            fm.created_token_id = tid;
                                            db.sys_file_mapping.Add(fm);
                                        //});
                                    }
                                }
                                if (listMessage.Count > 0)
                                {
                                    db.chat_message.AddRange(listMessage);
                                    db.SaveChanges();
                                }

                                if (listFileUp.Count > 0)
                                {
                                    db.chat_file.AddRange(listFileUp);
                                }

                                var membersend = db.chat_member.Where(z => z.user_join != uid && z.chat_group_id == chatGroupNow.chat_group_id).ToList();
                                foreach (chat_member m in membersend)
                                {
                                    m.number_not_seen = m.number_not_seen == null ? 1 : (m.number_not_seen + 1);
                                    m.status = 1;
                                }

                                List<string> nsids = new List<string>();
                                #region SendMS

                                #endregion sendMS
                                db.SaveChanges();

                                //if (listPathFileUp.Count > 0)
                                //{
                                //    foreach (var path in listPathFileUp)
                                //    {
                                //        if (System.IO.File.Exists(path))
                                //        {
                                //            System.IO.File.Delete(path);
                                //        }
                                //    }
                                //}
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", mess = listMesReturn, nsids = nsids });
                            }
                            else
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Cuộc hội thoại không tồn tại." });
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Add_Message", ip, tid, "Lỗi khi thêm mới tin nhắn", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Add_Message", ip, tid, "Lỗi khi thêm mới tin nhắn", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> Update_Message()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
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
                        string modelMessage = provider.FormData.GetValues("models").SingleOrDefault();
                        chat_message message = JsonConvert.DeserializeObject<chat_message>(modelMessage);
                        List<chat_message> listMessage = new List<chat_message>();
                        List<chat_message> listMesReturn = new List<chat_message>();
                        var chatGroupNow = db.chat_group.Find(message.chat_group_id);
                        if (chatGroupNow != null)
                        {
                            db.Entry(message).State = EntityState.Modified;
                            List<string> nsids = new List<string>();

                            db.SaveChanges();
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", mess = listMesReturn, nsids = nsids });
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Cuộc hội thoại không tồn tại." });
                        }
                        //return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Update_Message", ip, tid, "Lỗi khi cập nhật tin nhắn", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Update_Message", ip, tid, "Lỗi khi cập nhật tin nhắn", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Remove_Message([System.Web.Mvc.Bind(Include = "id")][FromBody] JObject data)
        {
            string id = data["id"].ToObject<string>();
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (id != null && id != "null" && id != "")
                    {
                        var me = await db.chat_member.FirstOrDefaultAsync(x => x.chat_group_id == id && x.user_join == uid);
                        if (me != null)
                        {
                            me.date_delete = DateTime.Now;
                            await db.SaveChangesAsync();
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Remove_Message", ip, tid, "Lỗi khi xóa lịch sử trò chuyện", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Remove_Message", ip, tid, "Lỗi khi xóa lịch sử trò chuyện", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Del_GroupChat([System.Web.Mvc.Bind(Include = "id")][FromBody] JObject data)
        {
            string id = data["id"].ToObject<string>();
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (id != null && id != "null" && id != "")
                    {
                        var me = await db.chat_group.FirstOrDefaultAsync(x => x.chat_group_id == id);
                        if (me != null)
                        {
                            me.status = -1;
                            await db.SaveChangesAsync();
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Del_GroupChat", ip, tid, "Lỗi khi xóa cuộc trò chuyện", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Del_GroupChat", ip, tid, "Lỗi khi xóa cuộc trò chuyện", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Out_GroupChat([System.Web.Mvc.Bind(Include = "user_leave_id,user_remove_id,ms_chat_group_id")][FromBody] JObject data)
        {
            string user_leave_id = data["user_leave_id"].ToObject<string>();
            string user_remove_id = data["user_remove_id"].ToObject<string>();
            string ms_chat_group_id = data["ms_chat_group_id"].ToObject<string>();
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    sys_users user_remove = new sys_users();
                    string noiDung = "";
                    // thanh vien leave
                    var user_leave = db.sys_users.Find(user_leave_id);

                    if (user_remove_id != null && user_remove_id.Trim() != "" && user_remove_id != "null")
                    {
                        user_remove = db.sys_users.Find(user_remove_id);
                        noiDung = user_remove.full_name + " vừa xoá " + user_leave.full_name + " khỏi nhóm chat.";
                    }
                    else
                    {
                        noiDung = user_leave.full_name + " vừa rời khỏi nhóm.";
                    }
                    var chat_group = db.chat_group.FirstOrDefault(x => x.chat_group_id == ms_chat_group_id);
                    #region message noti chat
                    chat_message da = new chat_message();
                    da.chat_message_id = helper.GenKey();
                    da.chat_group_id = ms_chat_group_id;
                    da.content_message = noiDung;
                    da.created_by = uid;
                    da.created_date = DateTime.Now;
                    da.created_ip = ip;
                    da.created_token_id = tid;
                    da.type_message = 5;
                    da.status = 1;
                    chat_group.modified_date = da.created_date;
                    db.chat_message.Add(da);
                    #endregion
                    #region out member
                    var member = await db.chat_member.FirstOrDefaultAsync(x => x.chat_group_id == da.chat_group_id && x.user_join == user_leave_id);
                    if (member != null)
                    {
                        db.chat_member.Remove(member);
                    }
                    #endregion
                    await db.SaveChangesAsync();

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Out_GroupChat", ip, tid, "Lỗi khi rời cuộc trò chuyện", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Out_GroupChat", ip, tid, "Lỗi khi rời cuộc trò chuyện", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Delete_Chat([System.Web.Mvc.Bind(Include = "id")][FromBody] JObject data)
        {
            string id = data["id"].ToObject<string>();
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (id != null && id != "null" && id != "")
                    {
                        var da = await db.chat_message.FindAsync(id);
                        if (da != null)
                        {
                            //db.chat_message.Remove(da);
                            da.status = -1;
                            da.deleted_by = uid;
                            da.deleted_date = DateTime.Now;
                            da.deleted_ip = ip;
                            da.deleted_token_id = tid;
                            var lfiles = await db.chat_file.Where(a => a.chat_message_id == da.chat_message_id).ToListAsync();
                            if (lfiles.Count > 0)
                            {
                                //var files = lfiles.Select(a => a.file_path).ToList();
                                //if (files.Count > 0)
                                //{
                                //    db.chat_file.RemoveRange(lfiles);
                                //    Task.Run(() =>
                                //    {
                                //        foreach (var f in files)
                                //        {
                                //            System.IO.Directory.Delete(helper.rootPath + f, true);
                                //        }
                                //    });
                                //}
                                foreach (var item in lfiles)
                                {
                                    item.deleted_by = uid;
                                    item.deleted_date = DateTime.Now;
                                    item.deleted_ip = ip;
                                    item.deleted_token_id = tid;
                                    item.is_delete = true;
                                }
                            }
                            var chat_recent = db.chat_message.Where(a => a.chat_group_id == da.chat_group_id).OrderByDescending(a => a.created_date).FirstOrDefault();
                            if (chat_recent != null)
                            {
                                if (chat_recent.chat_message_id == id)
                                {
                                    var chat_recent_del = db.chat_message.Where(a => a.chat_group_id == da.chat_group_id && a.status != -1 && a.chat_message_id != id).OrderByDescending(a => a.created_date).FirstOrDefault();
                                    if (chat_recent_del != null)
                                    {
                                        var chat_group_update = db.chat_group.Find(da.chat_group_id);
                                        chat_group_update.modified_date = chat_recent_del.created_date;
                                        chat_group_update.modified_by = chat_recent_del.created_by;
                                        chat_group_update.modified_ip = chat_recent_del.created_ip;
                                        chat_group_update.modified_token_id = chat_recent_del.created_token_id;
                                    }
                                }
                            }
                            var list_chat_user = db.chat_member.Where(a => a.chat_group_id == da.chat_group_id && a.user_join != uid).ToList();
                            if (list_chat_user.Count > 0)
                            {
                                foreach (var item in list_chat_user)
                                {
                                    item.number_not_seen = db.chat_message.Count(x => x.chat_group_id == da.chat_group_id && x.status != -1 && x.created_by != item.user_join && (item.last_seen_date != null ? (x.created_date > item.last_seen_date) : true))
                                                            - (item.last_seen_date != null ? (da.created_date > item.last_seen_date ? 1 : 0) : 1);
                                }
                            }
                            await db.SaveChangesAsync();
                        }

                        List<string> nsids = new List<string>();
                        var modelns = db.chat_member.Where(x => x.chat_group_id == da.chat_group_id).Select(x => x.user_join).ToList();
                        if (modelns.Count > 0)
                        {
                            foreach (var ids in modelns)
                            {
                                nsids.Add(ids);
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", da.chat_message_id, nsids = nsids });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Delete_Chat", ip, tid, "Lỗi khi xóa tin nhắn", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Delete_Chat", ip, tid, "Lỗi khi xóa tin nhắn", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Update_Stisk([System.Web.Mvc.Bind(Include = "chat_group_id,chat_message_id,stick_id")][FromBody] JObject data)
        {
            string chat_group_id = data["chat_group_id"].ToObject<string>();
            string chat_message_id = data["chat_message_id"].ToObject<string>();
            var stick_id = data["stick_id"]?.ToObject<int?>();
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    var da = await db.chat_stick.FirstOrDefaultAsync(a => a.chat_group_id == chat_group_id && a.chat_message_id == chat_message_id && a.created_by == uid);
                    if (da != null && da.stick_id == stick_id)
                    {
                        db.chat_stick.Remove(da);
                    }
                    else if (da != null)
                    {
                        da.stick_id = stick_id;
                        da.created_by = uid;
                        da.created_date = DateTime.Now;
                    }
                    else
                    {
                        var ta = new chat_stick();
                        ta.chat_stick_id = helper.GenKey();
                        ta.chat_group_id = chat_group_id;
                        ta.chat_message_id = chat_message_id;
                        ta.stick_id = stick_id;
                        ta.created_by = uid;
                        ta.created_date = DateTime.Now;
                        ta.created_ip = ip;
                        ta.created_token_id = tid;
                        db.chat_stick.Add(ta);
                    }
                    await db.SaveChangesAsync();
                    List<string> nsids = new List<string>();
                    // var ng = await db.sys_users.FindAsync(uid);

                    string ConnectionSQL = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                    var sqlpas = new List<SqlParameter>();
                    sqlpas.Add(new SqlParameter("@" + "chat_group_id", chat_group_id));
                    sqlpas.Add(new SqlParameter("@" + "users_ID", uid));
                    var arrpas = sqlpas.ToArray();
                    var task = SqlHelper.ExecuteDataset(ConnectionSQL, "chat_member_token", arrpas).Tables[0];
                    //var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(ConnectionSQL, "chat_member_token", arrpas).Tables[0]);
                    var tables = task;

                    List<UserToken> fbs = UserTokenByTable(tables);
                    foreach (var to in fbs)
                    {
                        nsids.Add(to.users_id);
                    }
                    var stick = await db.doc_ca_emotes.FindAsync(stick_id);

                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", MessageID = chat_message_id, created_by_user = uid, stick_file = stick?.emote_file, nsids = nsids });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Update_Stisk", ip, tid, "Lỗi khi cập nhật stick cho tin nhắn", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Update_Stisk", ip, tid, "Lỗi khi cập nhật stick cho tin nhắn", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Share_ChatMessage()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
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
                    string modelShare = "";

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        modelShare = provider.FormData.GetValues("models").SingleOrDefault();
                        chat_message da = JsonConvert.DeserializeObject<chat_message>(modelShare);
                        string listGroupChat = provider.FormData.GetValues("arrChatID").SingleOrDefault();
                        List<string> listID_Chat = JsonConvert.DeserializeObject<List<string>>(listGroupChat);
                        List<chat_message> mess = new List<chat_message>();
                        List<Users> users = new List<Users>();
                        List<LChats> chats = new List<LChats>();
                        if (listID_Chat.Count > 0)
                        {
                            foreach (var item in listID_Chat)
                            {
                                List<chat_message> messages = new List<chat_message>();
                                List<chat_file> filesMsg = new List<chat_file>();
                                if (item != null && item.Trim() != "")
                                {
                                    var chatGet = db.chat_group.Find(item);
                                    if (chatGet != null)
                                    {
                                        var memberChatNow = db.chat_member.FirstOrDefault(x => x.chat_group_id == item && x.user_join == uid);
                                        string id_member = memberChatNow != null ? memberChatNow.chat_member_id : null;
                                        string stringPath = "/Portals/" + organization_id_user + "/Chat/" + item.Trim();

                                        var listPathEdit = Regex.Replace(stringPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                        var pathEdit = "";
                                        foreach (var itemEdit in listPathEdit)
                                        {
                                            if (itemEdit.Trim() != "")
                                            {
                                                pathEdit += "/" + Path.GetFileName(itemEdit);
                                            }
                                        }
                                        var exist = System.IO.Directory.Exists(root + pathEdit);
                                        if (!exist)
                                        {
                                            System.IO.Directory.CreateDirectory(root + pathEdit);
                                        }
                                        var mestext = new chat_message();
                                        mestext.chat_message_id = helper.GenKey();
                                        mestext.chat_group_id = item;
                                        mestext.chat_member_id = id_member;
                                        mestext.created_by = uid;
                                        mestext.created_date = DateTime.Now;
                                        mestext.created_ip = ip;
                                        mestext.created_token_id = tid;
                                        mestext.chat_parent_id = null;
                                        mestext.content_message = da.content_message;
                                        mestext.type_message = da.type_message;
                                        mestext.status = 0;
                                        if (da.content_message != null && da.content_message != "")
                                        {
                                            mess.Add(new chat_message()
                                            {
                                                chat_group_id = item,
                                                chat_message_id = mestext.chat_message_id,
                                                chat_parent_id = null,
                                                type_message = mestext.type_message
                                            });
                                            messages.Add(mestext);
                                            memberChatNow.last_message_id = mestext.chat_message_id;
                                            memberChatNow.last_seen_date = DateTime.Now;
                                            memberChatNow.number_not_seen = 0;
                                            chatGet.modified_by = uid;
                                            chatGet.modified_date = DateTime.Now;
                                            chatGet.modified_ip = ip;
                                            chatGet.modified_token_id = tid;
                                        }

                                        var listFileMsg = db.chat_file.Where(x => x.chat_group_id == da.chat_group_id && x.chat_message_id == da.chat_message_id).ToList();
                                        foreach (var fileChatOld in listFileMsg)
                                        {
                                            if (fileChatOld != null)
                                            {
                                                string oldPath = fileChatOld.file_path;
                                                // Convert to unsign
                                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                                var fileName = pattern.Replace(helper.convertToUnSignChar(fileChatOld.file_name.Replace("%", "percent"), "_"), "");
                                                string newPath = stringPath + "/" + fileName;
                                                var existFile = System.IO.File.Exists(root + "/" + oldPath);
                                                if (!existFile)
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Thông tin chia sẻ không tồn tại, vui lòng kiểm tra lại" });
                                                }
                                                var listPathEdit_1 = Regex.Replace(newPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                                var pathEdit_1 = "";
                                                foreach (var itemEdit in listPathEdit_1)
                                                {
                                                    if (itemEdit.Trim() != "")
                                                    {
                                                        pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                                    }
                                                }
                                                System.IO.File.Copy(root + "/" + oldPath, root + "/" + pathEdit_1, true);
                                                //System.IO.File.Copy(root + "/" + oldPath, root + "/" + newPath, true);

                                                var mesimage = new chat_file();
                                                mesimage.file_id = helper.GenKey();
                                                mesimage.chat_group_id = item;
                                                mesimage.created_by = uid;
                                                mesimage.created_date = DateTime.Now;
                                                mesimage.created_ip = ip;
                                                mesimage.created_token_id = tid;
                                                mesimage.chat_message_id = mestext.chat_message_id;
                                                mesimage.is_image = da.type_message == 1 ? true : false;
                                                mesimage.file_name = fileChatOld.file_name;
                                                mesimage.file_type = fileChatOld.file_type;
                                                mesimage.file_size = fileChatOld.file_size;
                                                mesimage.file_path = newPath;
                                                filesMsg.Add(mesimage);
                                                if (da.content_message == null || da.content_message == "")
                                                {
                                                    mess.Add(new chat_message()
                                                    {
                                                        chat_group_id = item,
                                                        chat_message_id = mestext.chat_message_id,
                                                        chat_parent_id = null,
                                                        type_message = mestext.type_message
                                                    });
                                                    messages.Add(mestext);
                                                    memberChatNow.last_message_id = mestext.chat_message_id;
                                                    memberChatNow.last_seen_date = DateTime.Now;
                                                    memberChatNow.number_not_seen = 0;
                                                    chatGet.modified_by = uid;
                                                    chatGet.modified_date = DateTime.Now;
                                                    chatGet.modified_ip = ip;
                                                    chatGet.modified_token_id = tid;
                                                }
                                            }
                                        }

                                        if (messages.Count > 0)
                                        {
                                            db.chat_message.AddRange(messages);
                                            #region SendGM
                                            var lstsh = new List<sys_sendhub>();
                                            var ng = db.sys_users.Find(uid);

                                            string ConnectionSQL = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                                            var sqlpas = new List<SqlParameter>();
                                            sqlpas.Add(new SqlParameter("@" + "chat_group_id", item));
                                            sqlpas.Add(new SqlParameter("@" + "users_ID", uid));
                                            var arrpas = sqlpas.ToArray();
                                            var task = SqlHelper.ExecuteDataset(ConnectionSQL, "chat_member_token", arrpas).Tables[0];
                                            //var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(ConnectionSQL, "chat_member_token", arrpas).Tables[0]);
                                            var tables = task;

                                            List<UserToken> fbs = UserTokenByTable(tables);

                                            var content = "<soe>" + ng.full_name + "</soe> gửi một tin nhắn " + (chatGet.is_group_chat == true ? ("đến nhóm chat <soe>\"" + chatGet.chat_group_name + "\"</soe>") : "đến bạn");
                                            List<string> sendUsers = fbs.Select(x => x.users_id).ToList();
                                            helper.send_noti_chat(user_now.user_id, item, item, sendUsers, "Tin nhắn", content, 0, ip, tid);
                                            foreach (var to in fbs)
                                            {
                                                users.Add(new Users()
                                                {
                                                    chat_group_id = item,
                                                    user_id = to.users_id,
                                                });
                                            }
                                            #endregion
                                        }
                                        if (filesMsg.Count > 0)
                                        {
                                            db.chat_file.AddRange(filesMsg);
                                        }
                                        db.SaveChanges();
                                    }
                                }

                            }
                        }

                        string listUserChat = provider.FormData.GetValues("arrUser_ID").SingleOrDefault();
                        List<string> listID_UserChat = JsonConvert.DeserializeObject<List<string>>(listUserChat);
                        if (listID_UserChat.Count > 0)
                        {
                            foreach (var item in listID_UserChat)
                            {
                                List<chat_message> messages = new List<chat_message>();
                                List<chat_file> filesMsg = new List<chat_file>();
                                if (item != null && item.Trim() != "")
                                {
                                    var chatGet = db.chat_group.FirstOrDefault(x => x.is_group_chat != true && (x.user_chat == item || x.created_by == item));
                                    if (chatGet != null)
                                    {
                                        chats.Add(new LChats()
                                        {
                                            chat_group_id = chatGet.chat_group_id,
                                        });
                                        var memberChatNow = db.chat_member.FirstOrDefault(x => x.chat_group_id == chatGet.chat_group_id && x.user_join == item);
                                        string id_member = memberChatNow != null ? memberChatNow.chat_member_id : null;
                                        string stringPath = "/Portals/" + organization_id_user + "/Chat/" + chatGet.chat_group_id;

                                        var listPathEdit = Regex.Replace(stringPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                        var pathEdit = "";
                                        foreach (var itemEdit in listPathEdit)
                                        {
                                            if (itemEdit.Trim() != "")
                                            {
                                                pathEdit += "/" + Path.GetFileName(itemEdit);
                                            }
                                        }
                                        var exist = System.IO.Directory.Exists(root + pathEdit);
                                        if (!exist)
                                        {
                                            System.IO.Directory.CreateDirectory(root + pathEdit);
                                        }
                                        var mestext = new chat_message();
                                        mestext.chat_message_id = helper.GenKey();
                                        mestext.chat_group_id = chatGet.chat_group_id;
                                        mestext.chat_member_id = id_member;
                                        mestext.created_by = uid;
                                        mestext.created_date = DateTime.Now;
                                        mestext.created_ip = ip;
                                        mestext.created_token_id = tid;
                                        mestext.chat_parent_id = null;
                                        mestext.content_message = da.content_message;
                                        mestext.type_message = da.type_message;
                                        mestext.status = 0;
                                        if (da.content_message != null && da.content_message != "")
                                        {
                                            mess.Add(new chat_message()
                                            {
                                                chat_group_id = chatGet.chat_group_id,
                                                chat_message_id = mestext.chat_message_id,
                                                chat_parent_id = null,
                                                type_message = mestext.type_message
                                            });
                                            messages.Add(mestext);
                                            memberChatNow.last_message_id = mestext.chat_message_id;
                                            memberChatNow.last_seen_date = DateTime.Now;
                                            memberChatNow.number_not_seen = 0;
                                            chatGet.modified_by = uid;
                                            chatGet.modified_date = DateTime.Now;
                                            chatGet.modified_ip = ip;
                                            chatGet.modified_token_id = tid;
                                        }

                                        var listFileMsg = db.chat_file.Where(x => x.chat_group_id == da.chat_group_id && x.chat_message_id == da.chat_message_id).ToList();
                                        foreach (var fileChatOld in listFileMsg)
                                        {
                                            if (fileChatOld != null)
                                            {
                                                string oldPath = fileChatOld.file_path;
                                                // Convert to unsign
                                                Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                                var fileName = pattern.Replace(helper.convertToUnSignChar(fileChatOld.file_name.Replace("%", "percent"), "_"), "");
                                                string newPath = stringPath + "/" + fileName;
                                                var existFile = System.IO.File.Exists(root + "/" + oldPath);
                                                if (!existFile)
                                                {
                                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Thông tin chia sẻ không tồn tại, vui lòng kiểm tra lại" });
                                                }
                                                var listPathEdit_1 = Regex.Replace(newPath.Replace("\\", "/"), @"\.*/+", "/").Split('/');
                                                var pathEdit_1 = "";
                                                foreach (var itemEdit in listPathEdit_1)
                                                {
                                                    if (itemEdit.Trim() != "")
                                                    {
                                                        pathEdit_1 += "/" + Path.GetFileName(itemEdit);
                                                    }
                                                }
                                                System.IO.File.Copy(root + "/" + oldPath, root + "/" + pathEdit_1, true);
                                                //System.IO.File.Copy(root + "/" + oldPath, root + "/" + newPath, true);

                                                var mesimage = new chat_file();
                                                mesimage.file_id = helper.GenKey();
                                                mesimage.chat_group_id = chatGet.chat_group_id;
                                                mesimage.created_by = uid;
                                                mesimage.created_date = DateTime.Now;
                                                mesimage.created_ip = ip;
                                                mesimage.created_token_id = tid;
                                                mesimage.chat_message_id = mestext.chat_message_id;
                                                mesimage.is_image = da.type_message == 1 ? true : false;
                                                mesimage.file_name = fileChatOld.file_name;
                                                mesimage.file_type = fileChatOld.file_type;
                                                mesimage.file_size = fileChatOld.file_size;
                                                mesimage.file_path = newPath;
                                                filesMsg.Add(mesimage);
                                                if (da.content_message == null || da.content_message == "")
                                                {
                                                    mess.Add(new chat_message()
                                                    {
                                                        chat_group_id = chatGet.chat_group_id,
                                                        chat_message_id = mestext.chat_message_id,
                                                        chat_parent_id = null,
                                                        type_message = mestext.type_message
                                                    });
                                                    messages.Add(mestext);
                                                    memberChatNow.last_message_id = mestext.chat_message_id;
                                                    memberChatNow.last_seen_date = DateTime.Now;
                                                    memberChatNow.number_not_seen = 0;
                                                    chatGet.modified_by = uid;
                                                    chatGet.modified_date = DateTime.Now;
                                                    chatGet.modified_ip = ip;
                                                    chatGet.modified_token_id = tid;
                                                }
                                            }
                                        }

                                        if (messages.Count > 0)
                                        {
                                            db.chat_message.AddRange(messages);
                                            #region SendGM
                                            var lstsh = new List<sys_sendhub>();
                                            var ng = db.sys_users.Find(uid);

                                            string ConnectionSQL = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                                            var sqlpas = new List<SqlParameter>();
                                            sqlpas.Add(new SqlParameter("@" + "chat_group_id", chatGet.chat_group_id));
                                            sqlpas.Add(new SqlParameter("@" + "users_ID", uid));
                                            var arrpas = sqlpas.ToArray();
                                            var task = SqlHelper.ExecuteDataset(ConnectionSQL, "chat_member_token", arrpas).Tables[0];
                                            //var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(ConnectionSQL, "chat_member_token", arrpas).Tables[0]);
                                            var tables = task;

                                            List<UserToken> fbs = UserTokenByTable(tables);
                                            var content = "<soe>" + ng.full_name + "</soe> gửi một tin nhắn " + (chatGet.is_group_chat == true ? ("đến nhóm chat <soe>\"" + chatGet.chat_group_name + "\"</soe>") : "đến bạn");
                                            List<string> sendUsers = fbs.Select(x => x.users_id).ToList();
                                            helper.send_noti_chat(user_now.user_id, chatGet.chat_group_id, chatGet.chat_group_id, sendUsers, "Tin nhắn", content, 0, ip, tid);
                                            foreach (var to in fbs)
                                            {
                                                users.Add(new Users()
                                                {
                                                    chat_group_id = chatGet.chat_group_id,
                                                    user_id = to.users_id,
                                                });
                                            }
                                            #endregion
                                        }
                                        if (filesMsg.Count > 0)
                                        {
                                            db.chat_file.AddRange(filesMsg);
                                        }
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        var user_receive = db.sys_users.Find(item);
                                        if (user_receive != null)
                                        {
                                            chat_group new_chat = new chat_group();
                                            new_chat.chat_group_id = helper.GenKey();
                                            new_chat.chat_group_name = user_receive.full_name;
                                            new_chat.avatar_group = user_receive.avatar;
                                            new_chat.status = 1;
                                            new_chat.organization_id = user_receive.organization_id;
                                            new_chat.is_group_chat = false;
                                            new_chat.user_chat = user_receive.user_id;
                                            new_chat.created_by = uid;
                                            new_chat.created_date = DateTime.Now;
                                            new_chat.created_ip = ip;
                                            new_chat.created_token_id = tid;

                                            chats.Add(new LChats()
                                            {
                                                chat_group_id = new_chat.chat_group_id,
                                            });

                                            chat_member mem = new chat_member();
                                            mem.chat_member_id = helper.GenKey();
                                            mem.chat_group_id = new_chat.chat_group_id;
                                            mem.user_join = uid;
                                            mem.date_join = DateTime.Now;
                                            mem.date_out = null;
                                            mem.status = 1;
                                            mem.number_not_seen = 0;
                                            mem.is_captain = true;
                                            mem.is_notify = true;
                                            db.chat_member.Add(mem);

                                            chat_member mem_chat = new chat_member();
                                            mem_chat.chat_member_id = helper.GenKey();
                                            mem_chat.chat_group_id = new_chat.chat_group_id;
                                            mem_chat.user_join = new_chat.user_chat;
                                            mem_chat.date_join = DateTime.Now;
                                            mem_chat.date_out = null;
                                            mem_chat.status = 1;
                                            mem_chat.number_not_seen = 0;
                                            mem_chat.is_captain = false;
                                            mem_chat.is_notify = true;
                                            db.chat_member.Add(mem_chat);

                                            db.chat_group.Add(new_chat);
                                            db.SaveChanges();
                                            string stringPath = "/Portals/" + organization_id_user + "/Chat/" + new_chat.chat_group_id;
                                            var exist = System.IO.Directory.Exists(root + stringPath);
                                            if (!exist)
                                            {
                                                System.IO.Directory.CreateDirectory(root + stringPath);
                                            }
                                            var mestext = new chat_message();
                                            mestext.chat_message_id = helper.GenKey();
                                            mestext.chat_group_id = new_chat.chat_group_id;
                                            mestext.chat_member_id = mem.chat_member_id;
                                            mestext.created_by = uid;
                                            mestext.created_date = DateTime.Now;
                                            mestext.created_ip = ip;
                                            mestext.created_token_id = tid;
                                            mestext.chat_parent_id = null;
                                            mestext.content_message = da.content_message;
                                            mestext.type_message = da.type_message;
                                            mestext.status = 0;
                                            if (da.content_message != null && da.content_message != "")
                                            {
                                                mess.Add(new chat_message()
                                                {
                                                    chat_group_id = new_chat.chat_group_id,
                                                    chat_message_id = mestext.chat_message_id,
                                                    chat_parent_id = null,
                                                    type_message = mestext.type_message
                                                });
                                                messages.Add(mestext);
                                            }

                                            var listFileMsg = db.chat_file.Where(x => x.chat_group_id == da.chat_group_id && x.chat_message_id == da.chat_message_id).ToList();
                                            foreach (var fileChatOld in listFileMsg)
                                            {
                                                if (fileChatOld != null)
                                                {
                                                    string oldPath = fileChatOld.file_path;
                                                    // Convert to unsign
                                                    Regex pattern = new Regex("[;,~`/!@#$%^*+\\\t]");
                                                    var fileName = pattern.Replace(helper.convertToUnSignChar(fileChatOld.file_name.Replace("%", "percent"), "_"), "");
                                                    string newPath = stringPath + "/" + fileName;
                                                    var existFile = System.IO.File.Exists(root + "/" + oldPath);
                                                    if (!existFile)
                                                    {
                                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Thông tin chia sẻ không tồn tại, vui lòng kiểm tra lại" });
                                                    }
                                                    System.IO.File.Copy(root + "/" + oldPath, root + "/" + newPath, true);

                                                    var mesimage = new chat_file();
                                                    mesimage.file_id = helper.GenKey();
                                                    mesimage.chat_group_id = new_chat.chat_group_id;
                                                    mesimage.created_by = uid;
                                                    mesimage.created_date = DateTime.Now;
                                                    mesimage.created_ip = ip;
                                                    mesimage.created_token_id = tid;
                                                    mesimage.chat_message_id = mestext.chat_message_id;
                                                    mesimage.is_image = da.type_message == 1 ? true : false;
                                                    mesimage.file_name = fileChatOld.file_name;
                                                    mesimage.file_type = fileChatOld.file_type;
                                                    mesimage.file_size = fileChatOld.file_size;
                                                    mesimage.file_path = newPath;
                                                    filesMsg.Add(mesimage);
                                                    if (da.content_message == null || da.content_message == "")
                                                    {
                                                        mess.Add(new chat_message()
                                                        {
                                                            chat_group_id = new_chat.chat_group_id,
                                                            chat_message_id = mestext.chat_message_id,
                                                            chat_parent_id = null,
                                                            type_message = mestext.type_message
                                                        });
                                                        messages.Add(mestext);
                                                    }
                                                }
                                            }

                                            if (messages.Count > 0)
                                            {
                                                db.chat_message.AddRange(messages);
                                                #region SendGM
                                                var lstsh = new List<sys_sendhub>();
                                                var ng = db.sys_users.Find(uid);

                                                string ConnectionSQL = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
                                                var sqlpas = new List<SqlParameter>();
                                                sqlpas.Add(new SqlParameter("@" + "chat_group_id", new_chat.chat_group_id));
                                                sqlpas.Add(new SqlParameter("@" + "users_ID", uid));
                                                var arrpas = sqlpas.ToArray();
                                                var task = SqlHelper.ExecuteDataset(ConnectionSQL, "chat_member_token", arrpas).Tables[0];
                                                //var task = System.Threading.Tasks.Task.Run(() => SqlHelper.ExecuteDataset(ConnectionSQL, "chat_member_token", arrpas).Tables[0]);
                                                var tables = task;

                                                List<UserToken> fbs = UserTokenByTable(tables);

                                                var content = "<soe>" + ng.full_name + "</soe> gửi một tin nhắn " + (new_chat.is_group_chat == true ? ("đến nhóm chat <soe>\"" + new_chat.chat_group_name + "\"</soe>") : "đến bạn");
                                                List<string> sendUsers = fbs.Select(x => x.users_id).ToList();
                                                helper.send_noti_chat(user_now.user_id, new_chat.chat_group_id, new_chat.chat_group_id, sendUsers, "Tin nhắn", content, 0, ip, tid);
                                                foreach (var to in fbs)
                                                {
                                                    users.Add(new Users()
                                                    {
                                                        chat_group_id = new_chat.chat_group_id,
                                                        user_id = to.users_id,
                                                    });
                                                }
                                                #endregion
                                            }
                                            if (filesMsg.Count > 0)
                                            {
                                                db.chat_file.AddRange(filesMsg);
                                            }

                                            #region add chat_logs
                                            if (helper.wlog)
                                            {
                                                chat_logs log = new chat_logs();
                                                log.log_type = 0;
                                                log.message = "Thêm mới cuộc trò chuyện: " + new_chat.chat_group_name;
                                                log.chat_group_id = new_chat.chat_group_id;
                                                log.organization_id = new_chat.organization_id;
                                                log.created_date = DateTime.Now;
                                                log.created_by = uid;
                                                log.created_token_id = tid;
                                                log.created_ip = ip;
                                                log.is_view = false;
                                                db.chat_logs.Add(log);
                                            }
                                            #endregion
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", mess = mess, nsids = users, chats = chats });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Share_ChatMessage", ip, tid, "Lỗi khi chia sẻ tin nhắn", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Share_ChatMessage", ip, tid, "Lỗi khi chia sẻ tin nhắn", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Active_Notify([System.Web.Mvc.Bind(Include = "chat_group_id,is_notify")][FromBody] JObject data)
        {
            string id_chat = data["chat_group_id"].ToObject<string>();
            bool? is_notify = data["is_notify"]?.ToObject<bool?>();
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (id_chat != null && id_chat != "null" && id_chat != "")
                    {
                        var me = await db.chat_member.FirstOrDefaultAsync(x => x.chat_group_id == id_chat && x.user_join == uid);
                        if (me != null)
                        {
                            me.is_notify = is_notify != true ? true : false;
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Bạn không có quyền thực hiện chức năng này!" });
                        }
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Cuộc trò chuyện không tồn tại." });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Active_Notify", ip, tid, "Lỗi khi cập nhật trạng thái thông báo cho cuộc trò chuyện", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/Active_Notify", ip, tid, "Lỗi khi cập nhật trạng thái thông báo cho cuộc trò chuyện", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        #region reset noti chat
        //[HttpPut]
        //public async Task<HttpResponseMessage> ResetCountNotiChat()
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    IEnumerable<Claim> claims = identity.Claims;

        //    if (identity == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
        //    }

        //    string ip = getipaddress();
        //    string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
        //    string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
        //    string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
        //    string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
        //    try
        //    {
        //        using (DBEntities db = new DBEntities())
        //        {
        //            var user_reset = db.sys_users.Find(uid);
        //            if (user_reset != null)
        //            {
        //                user_reset.count_noti_chat = 0;
        //                DateTime sdate = DateTime.Now;
        //                DateTime edate = DateTime.Now;
        //                if (helper.wlog)
        //                {
        //                    sql_log log = new sql_log();
        //                    log.controller = domainurl + "chat/ResetCountNotiChat";
        //                    log.start_date = sdate;
        //                    log.end_date = edate;
        //                    log.milliseconds = (int)Math.Ceiling((edate - sdate).TotalMilliseconds);
        //                    log.user_id = uid;
        //                    log.token_id = tid;
        //                    log.created_ip = ip;
        //                    log.created_date = DateTime.Now;
        //                    log.created_by = uid;
        //                    log.created_token_id = tid;
        //                    log.modified_ip = ip;
        //                    log.modified_date = DateTime.Now;
        //                    log.modified_by = uid;
        //                    log.modified_token_id = tid;
        //                    log.full_name = name;
        //                    log.title = "Reset thông báo tin nhắn mới";
        //                    log.log_content = "user_id: " + uid;
        //                    db.sql_log.Add(log);
        //                }
        //                await db.SaveChangesAsync();
        //            }
        //            else
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1", ms = "Người dùng không tồn tại." });
        //            }
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
        //    }
        //    catch (DbEntityValidationException e)
        //    {
        //        string contents = helper.getCatchError(e, null);
        //        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "chat/ResetCountNotiChat", ip, tid, "Lỗi khi reset messages", 0, "chat");
        //        if (!helper.debug)
        //        {
        //            contents = helper.logCongtent;
        //        }
        //        Log.Error(contents);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
        //    }
        //    catch (Exception e)
        //    {
        //        string contents = helper.ExceptionMessage(e);
        //        helper.saveLog(uid, name, JsonConvert.SerializeObject(new { contents }), domainurl + "chat/ResetCountNotiChat", ip, tid, "Lỗi khi reset messages", 0, "chat");
        //        if (!helper.debug)
        //        {
        //            contents = helper.logCongtent;
        //        }
        //        Log.Error(contents);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
        //    }
        //}
        #endregion

        #endregion Chat new

        #region CallProc
        //[Authorize]
        [HttpPost]
        public async Task<HttpResponseMessage> GetDataProc([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            sqlProc proc = JsonConvert.DeserializeObject<sqlProc>(des);
            string nameErrProc = "Lỗi khi gọi proc";// + proc.proc + "'";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            //try
            //{
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
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
                        log.controller = domainurl + "chat/GetDataProc";
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
                return Request.CreateResponse(HttpStatusCode.OK, new { data = JSONresult, err = "0", proc_name = (helper.debug ? proc.proc : "") });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "chat/GetDataProc", ip, tid, nameErrProc, 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "chat/GetDataProc", ip, tid, nameErrProc, 0, "chat");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            //}
            //catch (Exception)
            //{
            //    return Request.CreateResponse(HttpStatusCode.BadRequest);
            //}

        }
        #endregion
        #region Scan file
        public async Task<HttpResponseMessage> ScanFileUpload()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            //try
            //{
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            //}
            //catch
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            //}
            try
            {
                string root = HttpContext.Current.Server.MapPath("~/");
                string strPath = root + "/Portals/CheckUpload";
                bool exists = Directory.Exists(strPath);
                if (!exists)
                {
                    Directory.CreateDirectory(strPath);
                }
                var provider = new MultipartFormDataStreamProvider(root + "/Portals/CheckUpload");
                // Read the form data and return an async task.
                var task = Request.Content.ReadAsMultipartAsync(provider).
                ContinueWith<HttpResponseMessage>(t =>
                {
                    if (t.IsFaulted || t.IsCanceled)
                    {
                        Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                    }
                    List<string> listPathFileUp = new List<string>();
                    bool hasVirus = false;
                    var resultCheck = "";
                    foreach (MultipartFileData fileData in provider.FileData)
                    {
                        var scanner = new AntiVirus.Scanner();
                        var resultScan = scanner.ScanAndClean(fileData.LocalFileName);
                        listPathFileUp.Add(fileData.LocalFileName);
                        resultCheck = resultScan.ToString();
                        if (resultScan.ToString() != "VirusNotFound")
                        {
                            hasVirus = true;
                        }
                    }
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
                    if (hasVirus)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Warning: File exists virus.", err = "1", ms1 = resultCheck });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0", ms1 = resultCheck });
                });
                return await task;
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/ScanFileUpload", ip, tid, "Lỗi khi scan file", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "chat/ScanFileUpload", ip, tid, "Lỗi khi scan file", 0, "chat");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        #endregion

        #region Send popup noti
        [HttpPost]
        public async Task<HttpResponseMessage> SendPopupNoti([System.Web.Mvc.Bind(Include = "str")][FromBody] JObject data)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string dataProc = data["str"].ToObject<string>();
            string des = Codec.DecryptString(dataProc, helper.psKey);
            popupNoti dataProp = JsonConvert.DeserializeObject<popupNoti>(des);
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                helper.send_popup_noti(uid, dataProp.title, dataProp.content, dataProp.uids, dataProp.image, domainurl + dataProp.url);
                return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = des, contents }), domainurl + "chat/SendPopupNoti", ip, tid, "Lỗi khi gọi popup", 0, "chat");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = des, contents }), domainurl + "chat/SendPopupNoti", ip, tid, "Lỗi khi gọi popup", 0, "chat");
                if (!helper.debug)
                {
                    contents = helper.logCongtent;
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }

        }
        #endregion
        public List<UserToken> UserTokenByTable([System.Web.Mvc.Bind(Include = "Rows")] DataTable dt)
        {
            List<UserToken> uts = new List<UserToken>();
            foreach (DataRow dr in dt.Rows)
            {
                UserToken ut = new UserToken();
                ut.users_id = dr["user_id"].ToString();
                ut.tokens = dr["tokens"].ToString();
                ut.full_name = dr["full_name"].ToString();
                ut.avatar = dr["avatar"].ToString();
                uts.Add(ut);
            }
            return uts;
        }
        public class UserToken
        {
            public string users_id { get; set; }
            public string full_name { get; set; }
            public string avatar { get; set; }
            public int is_type { get; set; }
            public string tokens { get; set; }
            public bool is_notify { get; set; }
        }
        public class Users
        {
            public string chat_group_id { get; set; }
            public string user_id { get; set; }
        }
        public class LChats
        {
            public string chat_group_id { get; set; }
        }
        // -------
    }
}
