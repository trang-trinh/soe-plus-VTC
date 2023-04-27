using API.Models;
using Helper;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

namespace API.Controllers.Leave
{
    public class hrm_leaveController : ApiController
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
            return HttpContext.Current.Request.UserHostAddress;
        }
        [HttpPut]
        public async Task<HttpResponseMessage> update_leave_profile()
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            try
            {
                if (identity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = "Bạn không có quyền truy cập chức năng này!", err = "1" });
                }
            }
            catch
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
                    // Provider
                    string rootTemp = HttpContext.Current.Server.MapPath("~/Portals");
                    bool existsTemp = Directory.Exists(rootTemp);
                    if (!existsTemp)
                        Directory.CreateDirectory(rootTemp);
                    var provider = new MultipartFormDataStreamProvider(rootTemp);
                    var task = await Request.Content.ReadAsMultipartAsync(provider);

                    // Params
                    var user_now = await db.sys_users.AsNoTracking().FirstOrDefaultAsync(x => x.user_id == uid);
                    string en_model = provider.FormData.GetValues("model").SingleOrDefault();
                    hrm_leave_profile model = JsonConvert.DeserializeObject<hrm_leave_profile>(en_model);

                    var leave = await db.hrm_leave_profile.FirstOrDefaultAsync(x => x.profile_id == model.profile_id && x.year == model.year);
                    if (leave != null) {
                        leave.month1 = model.month1;
                        leave.month2 = model.month2;
                        leave.month3 = model.month3;
                        leave.month4 = model.month4;
                        leave.month5 = model.month5;
                        leave.month6 = model.month6;
                        leave.month7 = model.month7;
                        leave.month8 = model.month8;
                        leave.month9 = model.month9;
                        leave.month10 = model.month10;
                        leave.month11 = model.month11;
                        leave.month12 = model.month12;
                        leave.modified_by = uid;
                        leave.modified_date = DateTime.Now;
                        leave.modified_ip = ip;
                        leave.modified_token_id = tid;
                    }
                    else
                    {
                        var profile = await db.hrm_profile.FirstOrDefaultAsync(x => x.profile_id == model.profile_id);
                        hrm_leave_profile md = new hrm_leave_profile();
                        md = model;
                        md.created_by = uid;
                        md.created_date = DateTime.Now;
                        md.created_ip = ip;
                        md.created_token_id = tid;
                        md.organization_id = profile.organization_id;
                        db.hrm_leave_profile.Add(md);
                    }
                    await db.SaveChangesAsync();
                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                }
            }
            catch (DbEntityValidationException e)
            {
                string contents = helper.getCatchError(e, null);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_leave/update_leave_profile", ip, tid, "Lỗi khi cập nhật", 0, "hrm_leave");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = contents }), domainurl + "hrm_leave/update_leave_profile", ip, tid, "Lỗi khi cập nhật", 0, "hrm_leave");
                if (!helper.debug)
                {
                    contents = "";
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> import_excel()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
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
                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/" + organization_id_user + "/Excel/");
                    bool exists = Directory.Exists(strPath);
                    if (!exists)
                        Directory.CreateDirectory(strPath);
                    var provider = new MultipartFormDataStreamProvider(strPath);
                    var task = Request.Content.ReadAsMultipartAsync(provider).ContinueWith<HttpResponseMessage>(t =>
                    {
                        if (t.IsFaulted || t.IsCanceled)
                        {
                            Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                        }
                        FileInfo finfo = new FileInfo(provider.FileData.First().LocalFileName);

                        string guid = Guid.NewGuid().ToString();

                        var fileNameTemp = Regex.Replace(finfo.FullName.Replace("\\", "/"), @"\.*/+", "/");
                        var listPathTemp = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        foreach (var item in listPathTemp)
                        {
                            if (item.Trim() != "")
                            {
                                pathConfigTemp += "/" + Path.GetFileName(item);
                            }
                        }
                        File.Move(pathConfigTemp.Substring(1), Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<hrm_leave_profile> dvs = new List<hrm_leave_profile>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                //var user_now = db.sys_users.FirstOrDefault(x => x.user_id == uid);
                                for (int i = 2; i <= ws.Dimension.End.Row; i++)
                                {
                                    int? organization_id = user_now.organization_id;
                                    if (ws.Cells[i, 2].Value == null)
                                    {
                                        break;
                                    }
                                    hrm_leave_profile dv = new hrm_leave_profile();
                                    for (int j = 3; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[1, j].Value == null)
                                        {
                                            break;
                                        }
                                        var column = ws.Cells[1, j].Value;
                                        var vl = ws.Cells[i, j].Value;
                                        switch (column)
                                        {
                                            case "MNS":
                                                var profile = db.hrm_profile.FirstOrDefault(x => x.profile_code == vl.ToString());
                                                if (profile != null)
                                                {
                                                    dv.profile_id = profile.profile_id;
                                                    organization_id = profile.organization_id;
                                                }
                                                break;
                                            case "Year":
                                                dv.year = vl != null ? int.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T1":
                                                dv.month1 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T2":
                                                dv.month2 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T3":
                                                dv.month3 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T4":
                                                dv.month4 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T5":
                                                dv.month5 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T6":
                                                dv.month6 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T7":
                                                dv.month7 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T8":
                                                dv.month8 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T9":
                                                dv.month9 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T10":
                                                dv.month10 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T11":
                                                dv.month11 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                            case "T12":
                                                dv.month12 = vl != null ? double.Parse(vl.ToString()) : 0;
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dv.organization_id = organization_id;
                                    
                                    if (!string.IsNullOrEmpty(dv.profile_id) && dv.year > 0)
                                    {
                                        var exists = db.hrm_leave_profile.FirstOrDefault(x => x.profile_id == dv.profile_id && x.year == dv.year);
                                        if (exists != null)
                                        {
                                            exists.month1 = dv.month1;
                                            exists.month2 = dv.month2;
                                            exists.month3 = dv.month3;
                                            exists.month4 = dv.month4;
                                            exists.month5 = dv.month5;
                                            exists.month6 = dv.month6;
                                            exists.month7 = dv.month7;
                                            exists.month8 = dv.month8;
                                            exists.month9 = dv.month9;
                                            exists.month10 = dv.month10;
                                            exists.month11 = dv.month11;
                                            exists.month12 = dv.month12;
                                        }
                                        else
                                        {
                                            dvs.Add(dv);
                                        }
                                    }
                                }
                                if (dvs.Count > 0)
                                {
                                    db.hrm_leave_profile.AddRange(dvs);
                                    db.SaveChanges();
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1" + e.Message });
                            }
                            catch (Exception e)
                            {
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = "1" + e });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = "0" });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "hrm_leave/import_excel", ip, tid, "Lỗi khi Import Excel", 0, "hrm_leave");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
                catch (Exception e)
                {
                    string contents = helper.ExceptionMessage(e);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "hrm_leave/import_excel", ip, tid, "Lỗi khi Import Excel", 0, "hrm_leave");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }
    }
}
