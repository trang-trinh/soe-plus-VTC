using System;
using System.Collections.Generic;
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
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using API.Helper;
using API.Models;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace API.Controllers.HRM.Campaign
{
    [Authorize(Roles = "login")]
    public class hrm_campage_processController : ApiController
    {
        public string getipaddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> add_hrm_campage_process()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }

            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");

                    string strPath = root + "/" + dvid + "/HRMProcess";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }

                        int type_send = int.Parse(provider.FormData.GetValues("type_send").SingleOrDefault());
                        int type_module = int.Parse(provider.FormData.GetValues("type_module").SingleOrDefault());
                        var key_id = int.Parse(provider.FormData.GetValues("key_id").SingleOrDefault());
                        string content = provider.FormData.GetValues("content").SingleOrDefault();
                        string hrm_obj = provider.FormData.GetValues("hrm_obj").SingleOrDefault();
                        //type_send=0: Quy trình , type_send=1: Nhóm duyệt, type_send=2: Cá nhân
                        //type_module=0: Đề xuất tuyển dụng, =1 Chiến dịch tuyển dụng
                        if (type_send == 0 && type_module == 0)
                        {
                            List<hrm_recruitment_proposal> hrm_Recruitment_Proposal = JsonConvert.DeserializeObject<List<hrm_recruitment_proposal>>(hrm_obj);
                            foreach (var item in hrm_Recruitment_Proposal)
                            {
                                var hrm_Recruitment = db.hrm_recruitment_proposal.Where(a => a.recruitment_proposal_id == item.recruitment_proposal_id).FirstOrDefault();
                                hrm_Recruitment.status = 1;
                                db.Entry(hrm_Recruitment).State = EntityState.Modified;
                                db.SaveChanges();

                                var stt = 1;
                                var sys_Process = db.sys_config_process.Where(a => a.config_process_id == key_id).FirstOrDefault();
                                var sys_Process_order = db.hrm_config_process_form.Where(a => a.key_id == item.recruitment_proposal_id &&
                                a.type_module==type_module
                                ).OrderByDescending(x=>x.is_order).FirstOrDefault();
                                var sttProcess = 0;
                                if (sys_Process_order != null)
                                {
                                    sttProcess = sys_Process_order.is_order;
                                }
                                if (sys_Process != null)
                                {
                                    //Tạo quy trình ngầm
                                    hrm_config_process_form hrm_Config_Process_Form = new hrm_config_process_form();
                                    hrm_Config_Process_Form.config_process_name = sys_Process.config_process_name;
                                    hrm_Config_Process_Form.key_id = item.recruitment_proposal_id;
                                    hrm_Config_Process_Form.type_module = type_module;
                                    hrm_Config_Process_Form.module = sys_Process.module;
                                    hrm_Config_Process_Form.config_process_type = sys_Process.config_process_type;
                                    hrm_Config_Process_Form.status = sys_Process.status;
                                    hrm_Config_Process_Form.is_order = sttProcess;
                                    hrm_Config_Process_Form.is_approved = false;
                                    hrm_Config_Process_Form.created_date = DateTime.Now;
                                    hrm_Config_Process_Form.created_by = uid;
                                    hrm_Config_Process_Form.created_ip = ip;
                                    hrm_Config_Process_Form.created_token_id = tid;
                                    db.hrm_config_process_form.Add(hrm_Config_Process_Form);
                                    db.SaveChanges();
                                    //Tạo nhóm duyệt người tạo
                                    hrm_config_approved_form hrm_Config_Approved_Created = new hrm_config_approved_form();
                                    hrm_Config_Approved_Created.approved_group_name = "Người gửi";
                                    hrm_Config_Approved_Created.approved_type = 0;
                                    hrm_Config_Approved_Created.is_approved = true;
                                    hrm_Config_Approved_Created.config_process_form_id = hrm_Config_Process_Form.config_process_form_id;
                                    hrm_Config_Approved_Created.date_approved = DateTime.Now;
                                    hrm_Config_Approved_Created.date_send = DateTime.Now;
                                    hrm_Config_Approved_Created.created_date = DateTime.Now;
                                    hrm_Config_Approved_Created.created_by = uid;
                                    hrm_Config_Approved_Created.created_ip = ip;
                                    hrm_Config_Approved_Created.created_token_id = tid;
                                    hrm_Config_Approved_Created.is_order = stt;
                                    db.hrm_config_approved_form.Add(hrm_Config_Approved_Created);
                                    db.SaveChanges();
                                    stt++;
                                    //Tạo người tạo
                                    hrm_config_approved_users_form hrm_Config_Approved_Users_Created = new hrm_config_approved_users_form();
                                    hrm_Config_Approved_Users_Created.date_approved = DateTime.Now;
                                    hrm_Config_Approved_Users_Created.is_approved = true;
                                    hrm_Config_Approved_Users_Created.content = content;
                                    hrm_Config_Approved_Users_Created.is_order = 0;
                                    hrm_Config_Approved_Users_Created.status = true;
                                    hrm_Config_Approved_Users_Created.config_approved_form_id = hrm_Config_Approved_Created.config_approved_form_id;
                                    hrm_Config_Approved_Users_Created.user_id = uid;
                                    hrm_Config_Approved_Users_Created.created_date = DateTime.Now;
                                    hrm_Config_Approved_Users_Created.created_by = uid;
                                    hrm_Config_Approved_Users_Created.created_ip = ip;
                                    hrm_Config_Approved_Users_Created.created_token_id = tid;
                                    db.hrm_config_approved_users_form.Add(hrm_Config_Approved_Users_Created);
                                    db.SaveChanges();
                                    #region Thêm File
                                    // This illustrates how to get thefile names.
                                    FileInfo fileInfo = null;
                                    MultipartFileData ffileData = null;
                                    string newFileName = "";
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
                                        newFileName = Path.Combine(root + "/" + dvid + "/HRMProcess", fileName);
                                        fileInfo = new FileInfo(newFileName);
                                        // if (fileInfo.Exists)
                                        // {
                                        //     fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                        //     fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                        //     newFileName = Path.Combine(root + "/" + dvid + "/HRMProcess", fileName);
                                        // }
                                           newFileName = Path.Combine(root + "/" + dvid + "/HRMProcess",
                                helper.newFileName(fileInfo, root + "/" + dvid + "/HRMProcess", newFileName, 1, root, int.Parse(dvid)));
                                        ffileData = fileData;
                                        if (fileInfo != null)
                                        {
                                            if (!Directory.Exists(fileInfo.Directory.FullName))
                                            {
                                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                                            }
                                            File.Move(ffileData.LocalFileName, newFileName);

                                        }
                                        hrm_file hrm_File = new hrm_file();
                                        hrm_File.file_name = Path.GetFileName(newFileName);
                                        hrm_File.key_id = hrm_Config_Approved_Users_Created.config_approved_users_form_id.ToString();
                                        hrm_File.file_path = "/Portals/" + dvid + "/HRMProcess/" + fileName;
                                        hrm_File.file_type = helper.GetFileExtension(fileName);
                                        var file_info = new FileInfo(strPath + "/" + fileName);
                                        hrm_File.file_size = file_info.Length;
                                        if (helper.IsImageFileName(newFileName))
                                        {
                                            hrm_File.is_image = true;
                                        }
                                        else
                                        {
                                            hrm_File.is_image = false;
                                        }
                                        hrm_File.is_type = 6;
                                        hrm_File.status = true;
                                        hrm_File.created_by = uid;
                                        hrm_File.created_date = DateTime.Now;
                                        hrm_File.organization_id = int.Parse(dvid);
                                        hrm_File.created_ip = ip;
                                        hrm_File.created_token_id = tid;
                                        db.hrm_file.Add(hrm_File);
db.SaveChanges();
                                    }
                                    #endregion


                                    //Tạo nhóm người duyệt theo quy trình
                                    var sys_Link_Approved = db.sys_process_link_approved.Where(a => a.config_process_id == key_id).OrderBy(x => x.is_order).ToList();
                                    var check_add = false;
                                    foreach (var indesx in sys_Link_Approved)
                                    {

                                        var sys_Approved = db.sys_approved_groups.Where(a => a.approved_groups_id == indesx.approved_groups_id).FirstOrDefault();
                                        if (sys_Approved != null)
                                        {
                                            hrm_config_approved_form hrm_Config_Approved_Form = new hrm_config_approved_form();
                                            hrm_Config_Approved_Form.is_order = stt;
                                            hrm_Config_Approved_Form.approved_group_name = sys_Approved.approved_group_name;
                                            hrm_Config_Approved_Form.approved_type = sys_Approved.approved_type;
                                            hrm_Config_Approved_Form.config_process_form_id = hrm_Config_Process_Form.config_process_form_id;
                                            hrm_Config_Approved_Form.is_approved = false;
                                            hrm_Config_Approved_Form.is_close = false;
                                            hrm_Config_Approved_Form.is_default = sys_Approved.is_default;
                                            hrm_Config_Approved_Form.is_local = sys_Approved.is_local;
                                            hrm_Config_Approved_Form.is_department = sys_Approved.is_department;
                                            hrm_Config_Approved_Form.is_return_created = sys_Approved.is_return_created;
                                            hrm_Config_Approved_Form.is_return_pre = sys_Approved.is_return_pre;
                                            hrm_Config_Approved_Form.created_date = DateTime.Now;
                                            hrm_Config_Approved_Form.created_by = uid;
                                            hrm_Config_Approved_Form.created_ip = ip;
                                            hrm_Config_Approved_Form.created_token_id = tid;
                                            db.hrm_config_approved_form.Add(hrm_Config_Approved_Form);
                                            db.SaveChanges();
                                            var sys_Approved_Users = db.sys_approved_users.Where(a => a.approved_groups_id == indesx.approved_groups_id).OrderBy(x => x.is_order).ToList();
                                            var stt_1 = 1;
                                            if ((hrm_Config_Process_Form.config_process_type == 1 || hrm_Config_Process_Form.config_process_type == 2) && hrm_Config_Approved_Form.approved_type == 2)
                                            {
                                                check_add = false;
                                            }
                                            foreach (var element in sys_Approved_Users)
                                            {
                                                hrm_config_approved_users_form hrm_Config_Approved_Users_Form = new hrm_config_approved_users_form();
                                                hrm_Config_Approved_Users_Form.config_approved_form_id = hrm_Config_Approved_Form.config_approved_form_id;
                                                hrm_Config_Approved_Users_Form.is_order = stt_1;
                                                hrm_Config_Approved_Users_Form.is_approved = false;
                                                hrm_Config_Approved_Users_Form.user_id = element.user_id;
                                                hrm_Config_Approved_Users_Form.created_date = DateTime.Now;
                                                hrm_Config_Approved_Users_Form.created_by = uid;
                                                hrm_Config_Approved_Users_Form.created_ip = ip;
                                                hrm_Config_Approved_Users_Form.created_token_id = tid;
                                                db.hrm_config_approved_users_form.Add(hrm_Config_Approved_Users_Form);
                                                db.SaveChanges();
                                                stt_1++;
                                                hrm_config_process hrm_Config_Process = new hrm_config_process();
                                                hrm_Config_Process.type_module = type_module;
                                                hrm_Config_Process.key_id = item.recruitment_proposal_id;
                                                hrm_Config_Process.config_process_name = item.recruitment_proposal_name;
                                                hrm_Config_Process.user_id = element.user_id;
                                                hrm_Config_Process.is_approved = false;
                                                hrm_Config_Process.users_form_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                hrm_Config_Process.aproved_groups_id = hrm_Config_Approved_Form.config_approved_form_id;
                                                hrm_Config_Process.process_form_id = hrm_Config_Process_Form.config_process_form_id;
                                                hrm_Config_Process.is_last = false;
                                                hrm_Config_Process.created_date = DateTime.Now;
                                                hrm_Config_Process.created_by = uid;
                                                hrm_Config_Process.created_ip = ip;
                                                hrm_Config_Process.created_token_id = tid;
                                                hrm_Config_Process.organization_id = int.Parse(dvid);
                                                hrm_Config_Process.pre_process_id = hrm_Config_Approved_Users_Created.config_approved_users_form_id;
                                                if (hrm_Config_Process_Form.config_process_type == 0 && hrm_Config_Approved_Form.approved_type == 1)
                                                {
                                                    if (check_add == false)
                                                    {
                                                        hrm_Config_Process.is_last = true;
                                                        db.hrm_config_process.Add(hrm_Config_Process);
                                                        db.SaveChanges();
                                                    }
                                                }

                                                if (hrm_Config_Process_Form.config_process_type == 0 && hrm_Config_Approved_Form.approved_type == 2)
                                                {
                                                    if (check_add == false)
                                                    {
                                                        db.hrm_config_process.Add(hrm_Config_Process);
                                                        db.SaveChanges();
                                                        check_add = true;
                                                    }
                                                }
                                                if (hrm_Config_Process_Form.config_process_type == 0 && hrm_Config_Approved_Form.approved_type == 3)
                                                {
                                                    if (check_add == false)
                                                    {
                                                        db.hrm_config_process.Add(hrm_Config_Process);
                                                        db.SaveChanges();
                                                    }
                                                }
                                                if ((hrm_Config_Process_Form.config_process_type == 1 || hrm_Config_Process_Form.config_process_type == 2) && hrm_Config_Approved_Form.approved_type == 1)
                                                {

                                                    hrm_Config_Process.is_last = true;
                                                    db.hrm_config_process.Add(hrm_Config_Process);
                                                    db.SaveChanges();

                                                }
                                                if ((hrm_Config_Process_Form.config_process_type == 1 || hrm_Config_Process_Form.config_process_type == 2) && hrm_Config_Approved_Form.approved_type == 2)
                                                {

                                                    if (check_add == false)
                                                    {
                                                        db.hrm_config_process.Add(hrm_Config_Process);
                                                        db.SaveChanges();
                                                        check_add = true;
                                                    }
                                                }
                                                if ((hrm_Config_Process_Form.config_process_type == 1 || hrm_Config_Process_Form.config_process_type == 2) && hrm_Config_Approved_Form.approved_type == 3)
                                                {
                                                    db.hrm_config_process.Add(hrm_Config_Process);
                                                    db.SaveChanges();

                                                }

                                            }
                                            if (hrm_Config_Process_Form.config_process_type == 0 && (hrm_Config_Approved_Form.approved_type == 1 || hrm_Config_Approved_Form.approved_type == 3))
                                            {
                                                check_add = true;
                                            }
                                        }
                                        stt++;
                                    }
                                }
                            }
                        }


                        //#region add hrm_log
                        //if (helper.wlog)
                        //{
                        //    hrm_log log = new hrm_log();
                        //    log.title = "Thêm chiến dịch tuyển dụng " + hrm_Campaign.campaign_name;

                        //    log.log_module = "hrm_Campaign";
                        //    log.log_type = 0;
                        //    log.id_key = hrm_Campaign.campaign_id.ToString();
                        //    log.created_date = DateTime.Now;
                        //    log.created_by = uid;
                        //    log.created_token_id = tid;
                        //    log.created_ip = ip;
                        //    db.hrm_log.Add(log);
                        //    db.SaveChanges();

                        //}
                        //#endregion
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "hrm_process/Add_hrm_process", ip, tid, "Lỗi khi thêm tiến trình", 0, "Tiến trình");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = "", contents }), domainurl + "hrm_process/Add_hrm_process", ip, tid, "Lỗi khi thêm tiến trình", 0, "Tiến trình");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_hrm_campage_process()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
            }
            string fdhrm_Campaign = "";
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool super = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();

            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string root = HttpContext.Current.Server.MapPath("~/Portals");

                    string strPath = root + "/" + dvid + "/HRMProcess";
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(root);

                    // Read the form data and return an async task.
                    var task = Request.Content.ReadAsMultipartAsync(provider).
                    ContinueWith<HttpResponseMessage>(t =>
                    {


                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }




                        var intw = int.Parse(dvid);

                        string content = provider.FormData.GetValues("content").SingleOrDefault();
                        string hrm_obj = provider.FormData.GetValues("hrm_obj").SingleOrDefault();


                        List<hrm_config_process> hrm_Configs = JsonConvert.DeserializeObject<List<hrm_config_process>>(hrm_obj);
                        foreach (var process in hrm_Configs)
                        {


                            var hrm_Config_Process = db.hrm_config_process.Where(a => a.config_process_id == process.config_process_id).FirstOrDefault();

                            if (hrm_Config_Process != null)
                            {
                                hrm_Config_Process.is_approved = true;
                                db.Entry(hrm_Config_Process).State = EntityState.Modified;
                                db.SaveChanges();
                                hrm_config_approved_users_form hrm_Config_Approved_Users_Form =
                                db.hrm_config_approved_users_form.Where(a => a.config_approved_users_form_id == hrm_Config_Process.users_form_id).FirstOrDefault();
                                if (hrm_Config_Approved_Users_Form != null)
                                {
                                    hrm_Config_Approved_Users_Form.is_approved = true;
                                    hrm_Config_Approved_Users_Form.content = content;
                                    hrm_Config_Approved_Users_Form.date_approved = DateTime.Now;
                                    db.Entry(hrm_Config_Approved_Users_Form).State = EntityState.Modified;
                                    db.SaveChanges();
                                    #region Thêm File
                                    FileInfo fileInfo = null;
                                    MultipartFileData ffileData = null;
                                    string newFileName = "";
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
                                        newFileName = Path.Combine(root + "/" + dvid + "/HRMProcess", fileName);
                                        fileInfo = new FileInfo(newFileName);
                                        // if (fileInfo.Exists)
                                        // {
                                        //     fileName = fileInfo.Name.Replace(fileInfo.Extension, "");
                                        //     fileName = fileName + (helper.ranNumberFile()) + fileInfo.Extension;

                                        //     newFileName = Path.Combine(root + "/" + dvid + "/HRMProcess", fileName);
                                        // }
                                               newFileName = Path.Combine(root + "/" + dvid + "/HRMProcess",
                                helper.newFileName(fileInfo, root + "/" + dvid + "/HRMProcess", newFileName, 1, root, int.Parse(dvid)));
                                        ffileData = fileData;
                                        if (fileInfo != null)
                                        {
                                            if (!Directory.Exists(fileInfo.Directory.FullName))
                                            {
                                                Directory.CreateDirectory(fileInfo.Directory.FullName);
                                            }
                                            File.Move(ffileData.LocalFileName, newFileName);

                                        }

                                        hrm_file hrm_File = new hrm_file();
                                        hrm_File.key_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id.ToString();
                                        hrm_File.file_name = Path.GetFileName(newFileName);
                                        hrm_File.file_path = "/Portals/" + dvid + "/HRMProcess/" + fileName;
                                        hrm_File.file_type = helper.GetFileExtension(fileName);
                                        var file_info = new FileInfo(strPath + "/" + fileName);
                                        hrm_File.file_size = file_info.Length;
                                        if (helper.IsImageFileName(newFileName))
                                        {
                                            hrm_File.is_image = true;
                                        }
                                        else
                                        {
                                            hrm_File.is_image = false;
                                        }
                                        hrm_File.is_type = 6;
                                        hrm_File.status = true;
                                        hrm_File.created_by = uid;
                                        hrm_File.created_date = DateTime.Now;
                                        hrm_File.created_ip = ip; hrm_File.organization_id = int.Parse(dvid);
                                        hrm_File.created_token_id = tid;
                                        db.hrm_file.Add(hrm_File);
db.SaveChanges();
                                    }
                                    #endregion
                                    var process_form = db.hrm_config_process_form.Where(a => a.config_process_form_id == hrm_Config_Process.process_form_id).FirstOrDefault();
                                    #region Tiến trình 1 nhiều
                                    if (process_form.config_process_type == 1)
                                    {
                                        hrm_config_approved_form hrm_Config_Approved_Form = db.hrm_config_approved_form.Where(a => a.config_approved_form_id
                                        == hrm_Config_Approved_Users_Form.config_approved_form_id)
                                        .OrderBy(x => x.is_order).FirstOrDefault();
                                        if (hrm_Config_Approved_Form != null)
                                        {
                                            //Kiểm tra loại duyệt 1: 1 nhiều, 2 Tuần tự, 3 Ngẫu nhiên
                                            if (hrm_Config_Approved_Form.approved_type == 1)
                                            {
                                                //Duyệt các tiến trình khác
                                                var AfterListProcess = db.hrm_config_process.Where(a => a.type_module == process.type_module &&
                                                                                                a.key_id == process.key_id && a.process_form_id == process.process_form_id).ToList();
                                    
                                                foreach (var item in AfterListProcess)
                                                {
                                                    item.is_approved = true;
                                                    db.Entry(item).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                                var hrm_Config_Approved_Form_List = db.hrm_config_approved_form.Where(a => a.config_process_form_id
                                             == hrm_Config_Approved_Form.config_process_form_id)
                                             .ToList();
                                                //Duyệt các nhóm duyệt khác

                                                foreach (var item in hrm_Config_Approved_Form_List)
                                                {
                                                    item.is_approved = true;
                                                    db.Entry(item).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                                //TIẾN trình 1 nhiều- Nhóm duyệt 1 - nhiều
                                                if (hrm_Config_Process.type_module == 0 && hrm_Config_Process.is_last == true && hrm_Config_Process.is_approved == true)
                                                {
                                                    var prososal_asd = db.hrm_recruitment_proposal.Where(a => a.recruitment_proposal_id == hrm_Config_Process.key_id).FirstOrDefault();
                                                    if (prososal_asd != null)
                                                    {
                                                        prososal_asd.status = 2;
                                                        db.Entry(prososal_asd).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                    }
                                                }


                                            }
                                            else if (hrm_Config_Approved_Form.approved_type == 2)
                                            {
                                                //Tiến trình 1 nhiều - Nhóm duyệt tuần tự
                                                #region Nhóm duyệt tuần tự
                                                var list_approved_users_form = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id == hrm_Config_Approved_Form.config_approved_form_id).ToList();
                                                var maxOrder = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id == hrm_Config_Approved_Form.config_approved_form_id).OrderByDescending(x => x.is_order).FirstOrDefault().is_order;
                                                if (hrm_Config_Approved_Users_Form.is_order == maxOrder && hrm_Config_Process.is_last == true)
                                                {
                                                    hrm_Config_Approved_Form.is_approved = true;
                                                    db.Entry(hrm_Config_Approved_Form).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                    //Duyệt các tiến trình khác
                                                    var AfterListProcess = db.hrm_config_process.Where(a => a.type_module == process.type_module &&
                                                                                                    a.key_id == process.key_id && a.process_form_id == process.process_form_id).ToList();

                                                    foreach (var item in AfterListProcess)
                                                    {
                                                        item.is_approved = true;
                                                        db.Entry(item).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                    }
                                                    var hrm_Config_Approved_Form_List = db.hrm_config_approved_form.Where(a => a.config_process_form_id
                                                 == hrm_Config_Approved_Form.config_process_form_id)
                                                 .ToList();
                                                    //Duyệt các nhóm duyệt khác

                                                    foreach (var item in hrm_Config_Approved_Form_List)
                                                    {
                                                        item.is_approved = true;
                                                        db.Entry(item).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                    }
                                                    if (hrm_Config_Process.type_module == 0 && hrm_Config_Process.is_last == true
                                                    && hrm_Config_Process.is_approved == true)
                                                    {
                                                  
                                                        var prososal_asd = db.hrm_recruitment_proposal.Where(a => a.recruitment_proposal_id == hrm_Config_Process.key_id).FirstOrDefault();
                                                        if (prososal_asd != null)
                                                        {
                                                            prososal_asd.status = 2;
                                                            db.Entry(prososal_asd).State = EntityState.Modified;
                                                            db.SaveChanges();
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (var item in list_approved_users_form)
                                                    {
                                                        if (item.is_order == hrm_Config_Approved_Users_Form.is_order + 1)
                                                        {
                                                            hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                            hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                            hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                            hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                            hrm_Config_Process_Add.user_id = item.user_id;
                                                            hrm_Config_Process_Add.is_approved = false;
                                                            hrm_Config_Process_Add.users_form_id = item.config_approved_users_form_id;
                                                            hrm_Config_Process_Add.aproved_groups_id = item.config_approved_form_id;
                                                            hrm_Config_Process_Add.process_form_id = hrm_Config_Approved_Form.config_process_form_id;
                                                            if (item.is_order == maxOrder)
                                                                hrm_Config_Process_Add.is_last = true;
                                                            else
                                                                hrm_Config_Process_Add.is_last = false;
                                                            hrm_Config_Process_Add.created_date = DateTime.Now;
                                                            hrm_Config_Process_Add.created_by = uid;
                                                            hrm_Config_Process_Add.created_ip = ip;
                                                            hrm_Config_Process_Add.created_token_id = tid;
                                                            hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                            hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                            db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                            db.SaveChanges();
                                                        }
                                                    }

                                                }

                                                #endregion
                                            }
                                            else if (hrm_Config_Approved_Form.approved_type == 3)
                                            {
                                                #region Nhóm duyệt ngẫu nhiên
                                                if (hrm_Config_Process.is_last == true)
                                                {
                                                    hrm_Config_Approved_Form.is_approved = true;
                                                    db.Entry(hrm_Config_Approved_Form).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                    //Duyệt các tiến trình khác
                                                    var AfterListProcess = db.hrm_config_process.Where(a => a.type_module == process.type_module &&
                                                                                                    a.key_id == process.key_id && a.process_form_id == process.process_form_id).ToList();

                                                    foreach (var item in AfterListProcess)
                                                    {
                                                        item.is_approved = true;
                                                        db.Entry(item).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                    }
                                                    var hrm_Config_Approved_Form_List = db.hrm_config_approved_form.Where(a => a.config_process_form_id
                                                 == hrm_Config_Approved_Form.config_process_form_id)
                                                 .ToList();
                                                    //Duyệt các nhóm duyệt khác

                                                    foreach (var item in hrm_Config_Approved_Form_List)
                                                    {
                                                        item.is_approved = true;
                                                        db.Entry(item).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                    }
                                                    if (hrm_Config_Process.type_module == 0 && hrm_Config_Process.is_last == true && hrm_Config_Process.is_approved == true)
                                                    {
                                                   
                                                        var prososal_asd = db.hrm_recruitment_proposal.Where(a => a.recruitment_proposal_id == hrm_Config_Process.key_id).FirstOrDefault();
                                                        if (prososal_asd != null)
                                                        {
                                                            prososal_asd.status = 2;
                                                            db.Entry(prososal_asd).State = EntityState.Modified;
                                                            db.SaveChanges();
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var CountUsersCheck = db.hrm_config_approved_users_form.AsNoTracking().Where(a => a.config_approved_form_id ==
                                                                                                      hrm_Config_Approved_Form.config_approved_form_id
                                                                                                      && a.is_approved == false
                                                                                                      ).ToList().Count();
                                                    if (CountUsersCheck == 1)
                                                    {
                                                        var User_last = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                       hrm_Config_Approved_Form.config_approved_form_id
                                                                                                       && a.is_approved == false
                                                                                                       ).FirstOrDefault();
                                                        var Process_last = db.hrm_config_process.Where(a => a.users_form_id ==
                                                                                                     User_last.config_approved_users_form_id
                                                                                                      && a.user_id == User_last.user_id
                                                                                                      ).FirstOrDefault();
                                                        Process_last.is_last = true;
                                                        db.Entry(Process_last).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                    }
                                                }
                                                #endregion

                                            }
                                            else
                                            {
                                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Có lỗi xảy ra! Vui lòng thử lại sau.", err = "1" });
                                            }

                                        }



                                    }
                                    #endregion
                                    #region Tiến trình Ngẫu nhiên
                                    if (process_form.config_process_type == 2)
                                    {
                                        hrm_config_approved_form hrm_Config_Approved_Form = db.hrm_config_approved_form.Where(a => a.config_approved_form_id == hrm_Config_Process.aproved_groups_id).OrderBy(x => x.is_order).FirstOrDefault();
                                        if (hrm_Config_Approved_Form != null)
                                        {
                                            //Kiểm tra loại duyệt 1: 1 - nhiều, 2 Tuần tự, 3 Ngẫu nhiên
                                            if (hrm_Config_Approved_Form.approved_type == 1)
                                            {
                                                var AfterListProcess = db.hrm_config_process.Where(a => a.aproved_groups_id ==
                                                                                                 process.aproved_groups_id &&
                                                                                                a.key_id == process.key_id && a.process_form_id == process.process_form_id).ToList();
                                                foreach (var item in AfterListProcess)
                                                {
                                                    item.is_approved = true;
                                                    db.Entry(item).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                                hrm_Config_Approved_Form.is_approved = true;
                                                db.Entry(hrm_Config_Approved_Form).State = EntityState.Modified;
                                                db.SaveChanges();
                                                //Chỗ này viết code nếu là cái trình duyệt cuối nhóm
                                                var maxOrderProcess = db.hrm_config_approved_form.Where(a => a.is_approved == true && a.config_process_form_id == hrm_Config_Approved_Form.config_process_form_id)
                                                .ToList().Count();
                                                var defOrderProcess = db.hrm_config_approved_form.Where(a => a.config_process_form_id == hrm_Config_Approved_Form.config_process_form_id)
                                               .ToList().Count();
                                                if (process_form != null)
                                                {
                                                    if (defOrderProcess == maxOrderProcess)
                                                    {
                                                        if (hrm_Config_Process.type_module == 0 && hrm_Config_Process.is_last == true && hrm_Config_Process.is_approved == true)
                                                        {
                                                            var prososal_asd = db.hrm_recruitment_proposal.Where(a => a.recruitment_proposal_id == hrm_Config_Process.key_id).FirstOrDefault();
                                                            if (prososal_asd != null)
                                                            {
                                                                prososal_asd.status = 2;
                                                                db.Entry(prososal_asd).State = EntityState.Modified;
                                                                db.SaveChanges();
                                                            }
                                                        }
                                                    }
                                               

                                                }

                                            }
                                            else if (hrm_Config_Approved_Form.approved_type == 2)
                                            {
                                                #region Nhóm duyệt tuần tự
                                                var list_approved_users_form = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id == hrm_Config_Approved_Form.config_approved_form_id).ToList();
                                                var maxOrder = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id == hrm_Config_Approved_Form.config_approved_form_id).OrderByDescending(x => x.is_order).FirstOrDefault().is_order;
                                                if (hrm_Config_Approved_Users_Form.is_order == maxOrder && hrm_Config_Process.is_last == true)
                                                {
                                                    hrm_Config_Approved_Form.is_approved = true;
                                                    db.Entry(hrm_Config_Approved_Form).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                    //Chỗ này viết code nếu là cái trình duyệt cuối nhóm
                                                    var maxOrderProcess = db.hrm_config_approved_form.Where(a => a.is_approved == true && a.config_process_form_id == hrm_Config_Approved_Form.config_process_form_id)
                                                    .ToList().Count();
                                                    var defOrderProcess = db.hrm_config_approved_form.Where(a => a.config_process_form_id == hrm_Config_Approved_Form.config_process_form_id)
                                                   .ToList().Count();
                                                     
                                                    if (defOrderProcess == maxOrderProcess)
                                                    {
                                                        if (hrm_Config_Process.type_module == 0 && hrm_Config_Process.is_last == true && hrm_Config_Process.is_approved == true)
                                                        {
                                                            var prososal_asd = db.hrm_recruitment_proposal.Where(a => a.recruitment_proposal_id == hrm_Config_Process.key_id).FirstOrDefault();
                                                            if (prososal_asd != null)
                                                            {
                                                                prososal_asd.status = 2;
                                                                db.Entry(prososal_asd).State = EntityState.Modified;
                                                                db.SaveChanges();
                                                            }
                                                        }
                                                    }
                                               }
                                                else
                                                {
                                                    foreach (var item in list_approved_users_form)
                                                    {
                                                        if (item.is_order == hrm_Config_Approved_Users_Form.is_order + 1)
                                                        {
                                                            hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                            hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                            hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                            hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                            hrm_Config_Process_Add.user_id = item.user_id;
                                                            hrm_Config_Process_Add.is_approved = false;
                                                            hrm_Config_Process_Add.users_form_id = item.config_approved_users_form_id;
                                                            hrm_Config_Process_Add.aproved_groups_id = item.config_approved_form_id;
                                                            hrm_Config_Process_Add.process_form_id = hrm_Config_Approved_Form.config_process_form_id;
                                                            if (item.is_order == maxOrder)
                                                                hrm_Config_Process_Add.is_last = true;
                                                            else
                                                                hrm_Config_Process_Add.is_last = false;
                                                            hrm_Config_Process_Add.created_date = DateTime.Now;
                                                            hrm_Config_Process_Add.created_by = uid;
                                                            hrm_Config_Process_Add.created_ip = ip;
                                                            hrm_Config_Process_Add.created_token_id = tid;
                                                            hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                            hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                            db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                            db.SaveChanges();
                                                        }
                                                    }

                                                }

                                                #endregion
                                            }
                                            else if (hrm_Config_Approved_Form.approved_type == 3)
                                            {
                                                #region Nhóm duyệt ngẫu nhiên
                                                if (hrm_Config_Process.is_last == true)
                                                {
                                                    hrm_Config_Approved_Form.is_approved = true;
                                                    db.Entry(hrm_Config_Approved_Form).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                    //Chỗ này viết code nếu là cái trình duyệt cuối nhóm
                                                    var maxOrderProcess = db.hrm_config_approved_form.Where(a => a.is_approved == true && a.config_process_form_id == hrm_Config_Approved_Form.config_process_form_id)
                                                    .ToList().Count();
                                                    var defOrderProcess = db.hrm_config_approved_form.Where(a => a.config_process_form_id == hrm_Config_Approved_Form.config_process_form_id)
                                                   .ToList().Count();

                                                    if (defOrderProcess == maxOrderProcess)
                                                    {
                                                        if (hrm_Config_Process.type_module == 0 && hrm_Config_Process.is_last == true && hrm_Config_Process.is_approved == true)
                                                        {
                                                            var prososal_asd = db.hrm_recruitment_proposal.Where(a => a.recruitment_proposal_id == hrm_Config_Process.key_id).FirstOrDefault();
                                                            if (prososal_asd != null)
                                                            {
                                                                prososal_asd.status = 2;
                                                                db.Entry(prososal_asd).State = EntityState.Modified;
                                                                db.SaveChanges();
                                                            }
                                                        }
                                                    }
                                                   }

                                                else
                                                {
                                                    var CountUsersCheck = db.hrm_config_approved_users_form.AsNoTracking().Where(a => a.config_approved_form_id ==
                                                                                                      hrm_Config_Approved_Form.config_approved_form_id
                                                                                                      && a.is_approved == false
                                                                                                      ).ToList().Count();

                                                    if (CountUsersCheck == 1)
                                                    {
                                                        var User_last = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                       hrm_Config_Approved_Form.config_approved_form_id
                                                                                                       && a.is_approved == false
                                                                                                       ).FirstOrDefault();
                                                        var Process_last = db.hrm_config_process.Where(a => a.users_form_id ==
                                                                                                     User_last.config_approved_users_form_id
                                                                                                      && a.user_id == User_last.user_id
                                                                                                      ).FirstOrDefault();
                                                        Process_last.is_last = true;
                                                        db.Entry(Process_last).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                    }
                                                }
                                                #endregion

                                            }
                                            else
                                            {
                                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Có lỗi xảy ra! Vui lòng thử lại sau.", err = "1" });
                                            }

                                        }
                                    }

                                    #endregion
                                    #region Tiến trình tuần tự
                                    if (process_form.config_process_type == 0)
                                    {
                                        hrm_config_approved_form hrm_Config_Approved_Form = db.hrm_config_approved_form.Where(a => a.config_approved_form_id == hrm_Config_Process.aproved_groups_id).OrderBy(x => x.is_order).FirstOrDefault();
                                        if (hrm_Config_Approved_Form != null)
                                        {
                                            //Kiểm tra loại duyệt 1: 1 nhiều, 2 Tuần tự, 3 Ngẫu nhiên
                                            if (hrm_Config_Approved_Form.approved_type == 1)
                                            {
                                                hrm_Config_Approved_Form.is_approved = true;
                                                db.Entry(hrm_Config_Approved_Form).State = EntityState.Modified;
                                                db.SaveChanges();
                                                var AfterListProcess = db.hrm_config_process.Where(a => a.aproved_groups_id ==
                                                                                                 process.aproved_groups_id &&
                                                                                                a.key_id == process.key_id && a.process_form_id == process.process_form_id).ToList();
                                                foreach (var item in AfterListProcess)
                                                {
                                                    item.is_approved = true;
                                                    db.Entry(item).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                }
                                                //Chỗ này viết code nếu là cái trình duyệt cuối nhóm
                                                var maxOrderProcess = db.hrm_config_approved_form.Where(a => a.config_process_form_id == hrm_Config_Approved_Form.config_process_form_id)
                                                .OrderByDescending(x => x.is_order).FirstOrDefault().is_order;
                                                if (process_form != null)
                                                {



                                                    if (hrm_Config_Approved_Form.is_order == maxOrderProcess)
                                                    {

                                                        if (hrm_Config_Process.type_module == 0 && hrm_Config_Process.is_last == true && hrm_Config_Process.is_approved == true)
                                                        {
                                                            var prososal_asd = db.hrm_recruitment_proposal.Where(a => a.recruitment_proposal_id == hrm_Config_Process.key_id).FirstOrDefault();
                                                            if (prososal_asd != null)
                                                            {
                                                                prososal_asd.status = 2;
                                                                db.Entry(prososal_asd).State = EntityState.Modified;
                                                                db.SaveChanges();
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {

                                                        var AfterAproves = db.hrm_config_approved_form.Where(a => a.is_order == (hrm_Config_Approved_Form.is_order + 1) && a.config_process_form_id ==
                                                        hrm_Config_Approved_Form.config_process_form_id).FirstOrDefault();
                                                        if (AfterAproves != null)
                                                        {
                                                            if (AfterAproves.approved_type == 1)
                                                            {
                                                                var AfterListUsers = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                         AfterAproves.config_approved_form_id).OrderBy(x => x.is_order).ToList();
                                                                foreach (var item in AfterListUsers)
                                                                {
                                                                    hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                                    hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                                    hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                                    hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                                    hrm_Config_Process_Add.user_id = item.user_id;
                                                                    hrm_Config_Process_Add.is_approved = false;
                                                                    hrm_Config_Process_Add.users_form_id = item.config_approved_users_form_id;
                                                                    hrm_Config_Process_Add.aproved_groups_id = item.config_approved_form_id;
                                                                    hrm_Config_Process_Add.process_form_id = AfterAproves.config_process_form_id;
                                                                    hrm_Config_Process_Add.is_last = true;
                                                                    hrm_Config_Process_Add.created_date = DateTime.Now;
                                                                    hrm_Config_Process_Add.created_by = uid;
                                                                    hrm_Config_Process_Add.created_ip = ip;
                                                                    hrm_Config_Process_Add.created_token_id = tid;
                                                                    hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                                    hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                                    db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                                    db.SaveChanges();
                                                                }

                                                            }
                                                            else if (AfterAproves.approved_type == 2)
                                                            {
                                                                var AfterUsers = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                              AfterAproves.config_approved_form_id).OrderBy(x => x.is_order).FirstOrDefault();
                                                                hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                                hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                                hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                                hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                                hrm_Config_Process_Add.user_id = AfterUsers.user_id;
                                                                hrm_Config_Process_Add.is_approved = false;
                                                                hrm_Config_Process_Add.users_form_id = AfterUsers.config_approved_users_form_id;
                                                                hrm_Config_Process_Add.aproved_groups_id = AfterUsers.config_approved_form_id;
                                                                hrm_Config_Process_Add.process_form_id = AfterAproves.config_process_form_id;
                                                                hrm_Config_Process_Add.is_last = false;
                                                                hrm_Config_Process_Add.created_date = DateTime.Now;
                                                                hrm_Config_Process_Add.created_by = uid;
                                                                hrm_Config_Process_Add.created_ip = ip;
                                                                hrm_Config_Process_Add.created_token_id = tid;
                                                                hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                                hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                                db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                                db.SaveChanges();
                                                            }
                                                            else if (AfterAproves.approved_type == 3)
                                                            {
                                                                var AfterListUsers = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                          AfterAproves.config_approved_form_id).OrderBy(x => x.is_order).ToList();
                                                                foreach (var item in AfterListUsers)
                                                                {
                                                                    hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                                    hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                                    hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                                    hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                                    hrm_Config_Process_Add.user_id = item.user_id;
                                                                    hrm_Config_Process_Add.is_approved = false;
                                                                    hrm_Config_Process_Add.users_form_id = item.config_approved_users_form_id;
                                                                    hrm_Config_Process_Add.aproved_groups_id = item.config_approved_form_id;
                                                                    hrm_Config_Process_Add.process_form_id = AfterAproves.config_process_form_id;
                                                                    hrm_Config_Process_Add.is_last = false;
                                                                    hrm_Config_Process_Add.created_date = DateTime.Now;
                                                                    hrm_Config_Process_Add.created_by = uid;
                                                                    hrm_Config_Process_Add.created_ip = ip;
                                                                    hrm_Config_Process_Add.created_token_id = tid;
                                                                    hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                                    hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                                    db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                                    db.SaveChanges();
                                                                }
                                                            }
                                                        }
                                                    }


                                                }

                                            }
                                            else if (hrm_Config_Approved_Form.approved_type == 2)
                                            {
                                                #region Nhóm duyệt tuần tự
                                                var list_approved_users_form = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id == hrm_Config_Approved_Form.config_approved_form_id).ToList();
                                                var maxOrder = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id == hrm_Config_Approved_Form.config_approved_form_id).OrderByDescending(x => x.is_order).FirstOrDefault().is_order;
                                                if (hrm_Config_Approved_Users_Form.is_order == maxOrder && hrm_Config_Process.is_last == true)
                                                {
                                                    hrm_Config_Approved_Form.is_approved = true;
                                                    db.Entry(hrm_Config_Approved_Form).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                    //Chỗ này viết code nếu là cái trình duyệt cuối nhóm
                                                    var maxOrderProcess = db.hrm_config_approved_form.Where(a => a.config_process_form_id == hrm_Config_Approved_Form.config_process_form_id)
                                                    .OrderByDescending(x => x.is_order).FirstOrDefault().is_order;
                                                    if (hrm_Config_Approved_Form.is_order == maxOrderProcess)
                                                    {
                                                        if (hrm_Config_Process.type_module == 0 && hrm_Config_Process.is_last == true && hrm_Config_Process.is_approved == true)
                                                        {
                                                            var prososal_asd = db.hrm_recruitment_proposal.Where(a => a.recruitment_proposal_id == hrm_Config_Process.key_id).FirstOrDefault();
                                                            if (prososal_asd != null)
                                                            {
                                                                prososal_asd.status = 2;
                                                                db.Entry(prososal_asd).State = EntityState.Modified;
                                                                db.SaveChanges();
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var AfterAproves = db.hrm_config_approved_form.Where(a => a.is_order == (hrm_Config_Approved_Form.is_order + 1) && a.config_process_form_id ==
                                                        hrm_Config_Approved_Form.config_process_form_id).FirstOrDefault();
                                                        if (AfterAproves != null)
                                                        {
                                                            if (AfterAproves.approved_type == 1)
                                                            {
                                                                var AfterListUsers = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                         AfterAproves.config_approved_form_id).OrderBy(x => x.is_order).ToList();
                                                                foreach (var item in AfterListUsers)
                                                                {
                                                                    hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                                    hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                                    hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                                    hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                                    hrm_Config_Process_Add.user_id = item.user_id;
                                                                    hrm_Config_Process_Add.is_approved = false;
                                                                    hrm_Config_Process_Add.users_form_id = item.config_approved_users_form_id;
                                                                    hrm_Config_Process_Add.aproved_groups_id = item.config_approved_form_id;
                                                                    hrm_Config_Process_Add.process_form_id = AfterAproves.config_process_form_id;
                                                                    hrm_Config_Process_Add.is_last = true;
                                                                    hrm_Config_Process_Add.created_date = DateTime.Now;
                                                                    hrm_Config_Process_Add.created_by = uid;
                                                                    hrm_Config_Process_Add.created_ip = ip;
                                                                    hrm_Config_Process_Add.created_token_id = tid;
                                                                    hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                                    hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                                    db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                                    db.SaveChanges();
                                                                }

                                                            }
                                                            else if (AfterAproves.approved_type == 2)
                                                            {
                                                                var AfterUsers = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                              AfterAproves.config_approved_form_id).OrderBy(x => x.is_order).FirstOrDefault();
                                                                hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                                hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                                hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                                hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                                hrm_Config_Process_Add.user_id = AfterUsers.user_id;
                                                                hrm_Config_Process_Add.is_approved = false;
                                                                hrm_Config_Process_Add.users_form_id = AfterUsers.config_approved_users_form_id;
                                                                hrm_Config_Process_Add.aproved_groups_id = AfterUsers.config_approved_form_id;
                                                                hrm_Config_Process_Add.process_form_id = AfterAproves.config_process_form_id;
                                                                hrm_Config_Process_Add.is_last = false;
                                                                hrm_Config_Process_Add.created_date = DateTime.Now;
                                                                hrm_Config_Process_Add.created_by = uid;
                                                                hrm_Config_Process_Add.created_ip = ip;
                                                                hrm_Config_Process_Add.created_token_id = tid;
                                                                hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                                hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                                db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                                db.SaveChanges();
                                                            }
                                                            else if (AfterAproves.approved_type == 3)
                                                            {
                                                                var AfterListUsers = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                          AfterAproves.config_approved_form_id).OrderBy(x => x.is_order).ToList();
                                                                foreach (var item in AfterListUsers)
                                                                {
                                                                    hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                                    hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                                    hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                                    hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                                    hrm_Config_Process_Add.user_id = item.user_id;
                                                                    hrm_Config_Process_Add.is_approved = false;
                                                                    hrm_Config_Process_Add.users_form_id = item.config_approved_users_form_id;
                                                                    hrm_Config_Process_Add.aproved_groups_id = item.config_approved_form_id;
                                                                    hrm_Config_Process_Add.process_form_id = AfterAproves.config_process_form_id;
                                                                    hrm_Config_Process_Add.is_last = false;
                                                                    hrm_Config_Process_Add.created_date = DateTime.Now;
                                                                    hrm_Config_Process_Add.created_by = uid;
                                                                    hrm_Config_Process_Add.created_ip = ip;
                                                                    hrm_Config_Process_Add.created_token_id = tid;
                                                                    hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                                    hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                                    db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                                    db.SaveChanges();
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (var item in list_approved_users_form)
                                                    {
                                                        if (item.is_order == hrm_Config_Approved_Users_Form.is_order + 1)
                                                        {
                                                            hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                            hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                            hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                            hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                            hrm_Config_Process_Add.user_id = item.user_id;
                                                            hrm_Config_Process_Add.is_approved = false;
                                                            hrm_Config_Process_Add.users_form_id = item.config_approved_users_form_id;
                                                            hrm_Config_Process_Add.aproved_groups_id = item.config_approved_form_id;
                                                            hrm_Config_Process_Add.process_form_id = hrm_Config_Approved_Form.config_process_form_id;
                                                            if (item.is_order == maxOrder)
                                                                hrm_Config_Process_Add.is_last = true;
                                                            else
                                                                hrm_Config_Process_Add.is_last = false;
                                                            hrm_Config_Process_Add.created_date = DateTime.Now;
                                                            hrm_Config_Process_Add.created_by = uid;
                                                            hrm_Config_Process_Add.created_ip = ip;
                                                            hrm_Config_Process_Add.created_token_id = tid;
                                                            hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                            hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                            db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                            db.SaveChanges();
                                                        }
                                                    }

                                                }

                                                #endregion
                                            }
                                            else if (hrm_Config_Approved_Form.approved_type == 3)
                                            {
                                                #region Nhóm duyệt ngẫu nhiên
                                                if (hrm_Config_Process.is_last == true)
                                                {
                                                    hrm_Config_Approved_Form.is_approved = true;
                                                    db.Entry(hrm_Config_Approved_Form).State = EntityState.Modified;
                                                    db.SaveChanges();
                                                    //Chỗ này viết code nếu là cái trình duyệt cuối nhóm
                                                    var maxOrderProcess = db.hrm_config_approved_form.Where(a => a.config_process_form_id == hrm_Config_Approved_Form.config_process_form_id)
                                                    .OrderByDescending(x => x.is_order).FirstOrDefault().is_order;
                                                    if (hrm_Config_Approved_Form.is_order == maxOrderProcess)
                                                    {
                                                        if (hrm_Config_Process.type_module == 0 && hrm_Config_Process.is_last == true && hrm_Config_Process.is_approved == true)
                                                        {
                                                            var prososal_asd = db.hrm_recruitment_proposal.Where(a => a.recruitment_proposal_id == hrm_Config_Process.key_id).FirstOrDefault();
                                                            if (prososal_asd != null)
                                                            {
                                                                prososal_asd.status = 2;
                                                                db.Entry(prososal_asd).State = EntityState.Modified;
                                                                db.SaveChanges();
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var AfterAproves = db.hrm_config_approved_form.Where(a => a.is_order == (hrm_Config_Approved_Form.is_order + 1) && a.config_process_form_id ==
                                                        hrm_Config_Approved_Form.config_process_form_id).FirstOrDefault();
                                                        if (AfterAproves != null)
                                                        {
                                                            if (AfterAproves.approved_type == 1)
                                                            {
                                                                var AfterListUsers = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                         AfterAproves.config_approved_form_id).OrderBy(x => x.is_order).ToList();
                                                                foreach (var item in AfterListUsers)
                                                                {
                                                                    hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                                    hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                                    hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                                    hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                                    hrm_Config_Process_Add.user_id = item.user_id;
                                                                    hrm_Config_Process_Add.is_approved = false;
                                                                    hrm_Config_Process_Add.users_form_id = item.config_approved_users_form_id;
                                                                    hrm_Config_Process_Add.aproved_groups_id = item.config_approved_form_id;
                                                                    hrm_Config_Process_Add.process_form_id = AfterAproves.config_process_form_id;
                                                                    hrm_Config_Process_Add.is_last = true;
                                                                    hrm_Config_Process_Add.created_date = DateTime.Now;
                                                                    hrm_Config_Process_Add.created_by = uid;
                                                                    hrm_Config_Process_Add.created_ip = ip;
                                                                    hrm_Config_Process_Add.created_token_id = tid;
                                                                    hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                                    hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                                    db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                                    db.SaveChanges();
                                                                }

                                                            }
                                                            else if (AfterAproves.approved_type == 2)
                                                            {
                                                                var AfterUsers = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                              AfterAproves.config_approved_form_id).OrderBy(x => x.is_order).FirstOrDefault();
                                                                hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                                hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                                hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                                hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                                hrm_Config_Process_Add.user_id = AfterUsers.user_id;
                                                                hrm_Config_Process_Add.is_approved = false;
                                                                hrm_Config_Process_Add.users_form_id = AfterUsers.config_approved_users_form_id;
                                                                hrm_Config_Process_Add.aproved_groups_id = AfterUsers.config_approved_form_id;
                                                                hrm_Config_Process_Add.process_form_id = AfterAproves.config_process_form_id;
                                                                hrm_Config_Process_Add.is_last = false;
                                                                hrm_Config_Process_Add.created_date = DateTime.Now;
                                                                hrm_Config_Process_Add.created_by = uid;
                                                                hrm_Config_Process_Add.created_ip = ip;
                                                                hrm_Config_Process_Add.created_token_id = tid;
                                                                hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                                hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                                db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                                db.SaveChanges();
                                                            }
                                                            else if (AfterAproves.approved_type == 3)
                                                            {
                                                                var AfterListUsers = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                          AfterAproves.config_approved_form_id).OrderBy(x => x.is_order).ToList();
                                                                foreach (var item in AfterListUsers)
                                                                {
                                                                    hrm_config_process hrm_Config_Process_Add = new hrm_config_process();
                                                                    hrm_Config_Process_Add.type_module = hrm_Config_Process.type_module;
                                                                    hrm_Config_Process_Add.key_id = hrm_Config_Process.key_id;
                                                                    hrm_Config_Process_Add.config_process_name = hrm_Config_Process.config_process_name;
                                                                    hrm_Config_Process_Add.user_id = item.user_id;
                                                                    hrm_Config_Process_Add.is_approved = false;
                                                                    hrm_Config_Process_Add.users_form_id = item.config_approved_users_form_id;
                                                                    hrm_Config_Process_Add.aproved_groups_id = item.config_approved_form_id;
                                                                    hrm_Config_Process_Add.process_form_id = AfterAproves.config_process_form_id;
                                                                    hrm_Config_Process_Add.is_last = false;
                                                                    hrm_Config_Process_Add.created_date = DateTime.Now;
                                                                    hrm_Config_Process_Add.created_by = uid;
                                                                    hrm_Config_Process_Add.created_ip = ip;
                                                                    hrm_Config_Process_Add.created_token_id = tid;
                                                                    hrm_Config_Process_Add.organization_id = int.Parse(dvid);
                                                                    hrm_Config_Process_Add.pre_process_id = hrm_Config_Approved_Users_Form.config_approved_users_form_id;
                                                                    db.hrm_config_process.Add(hrm_Config_Process_Add);
                                                                    db.SaveChanges();
                                                                }
                                                            }
                                                        }
                                                    }

                                                }

                                                else
                                                {
                                                    var CountUsersCheck = db.hrm_config_approved_users_form.AsNoTracking().Where(a => a.config_approved_form_id ==
                                                                                                      hrm_Config_Approved_Form.config_approved_form_id
                                                                                                      && a.is_approved == false
                                                                                                      ).ToList().Count();

                                                    if (CountUsersCheck == 1)
                                                    {
                                                        var User_last = db.hrm_config_approved_users_form.Where(a => a.config_approved_form_id ==
                                                                                                       hrm_Config_Approved_Form.config_approved_form_id
                                                                                                       && a.is_approved == false
                                                                                                       ).FirstOrDefault();
                                                        var Process_last = db.hrm_config_process.Where(a => a.users_form_id ==
                                                                                                     User_last.config_approved_users_form_id
                                                                                                      && a.user_id == User_last.user_id
                                                                                                      ).FirstOrDefault();
                                                        Process_last.is_last = true;
                                                        db.Entry(Process_last).State = EntityState.Modified;
                                                        db.SaveChanges();
                                                    }
                                                }
                                                #endregion

                                            }
                                            else
                                            {
                                                return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Có lỗi xảy ra! Vui lòng thử lại sau.", err = "1" });
                                            }

                                        }
                                    }
                                    #endregion
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_Campaign, contents }), domainurl + "hrm_campaign/Update_hrm_Campaign", ip, tid, "Lỗi khi cập nhật hrm_Campaign", 0, "hrm_Campaign");
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
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fdhrm_Campaign, contents }), domainurl + "hrm_campaign/Update_hrm_Campaign", ip, tid, "Lỗi khi cập nhật hrm_Campaign", 0, "hrm_Campaign");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }


    }
}
