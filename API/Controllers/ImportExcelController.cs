using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
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
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Text;

namespace API.Controllers
{
    [Authorize(Roles = "login")]
    public class ImportExcelController : ApiController
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
        #region Từ điển
        public int getID([System.Web.Mvc.Bind(Include = "")] int id, [System.Web.Mvc.Bind(Include = "")] string name)
        {
            int rid = 0;
            string fname = name;
            using (DBEntities db = new DBEntities())
            {
                var orgs = db.sys_organization.Where(x => x.parent_id == id).ToArray();
                if (orgs != null)
                {
                    foreach (var org in orgs)
                    {
                        if (org.organization_name == fname)
                        {
                            rid = org.organization_id;
                            return rid;
                        }
                        else
                        {
                            var dpm = db.sys_organization.Where(x => x.parent_id == org.organization_id).ToArray();
                            foreach (var dp in dpm)
                            {
                                if (dp.organization_name == fname)
                                {
                                    rid = dp.organization_id;
                                    return rid;
                                }
                                else
                                {
                                    getID(dp.organization_id, fname);
                                }
                            }
                        }
                    }

                }
            }
            if (rid > 0)
                return rid;
            else return 0;
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Import_Place()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<ca_places> dvs = new List<ca_places>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                string col_name = ws.Cells[4, 1].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "TỈNH THÀNH PHỐ") // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                {
                                    errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    listErr += ws.Cells[i, 1].Value != null && ws.Cells[i, 2].Value.ToString().Length > 500 ? "Dòng thứ " + i + " cột <b>Tên địa danh</b> không quá 500 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 500 ? "Dòng thứ " + i + " cột <b>Tên địa danh</b> không quá 500 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 3].Value != null && ws.Cells[i, 2].Value.ToString().Length > 500 ? "Dòng thứ " + i + " cột <b>Tên địa danh</b> không quá 500 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>Tỉnh Thành phố</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Quận Huyện</b> không được để trống <br>" : null;
                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    ca_places dv = new ca_places(); int city_ID = 0;

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.ca_places.Count();

                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var vl = ws.Cells[i, j].Value;
                                        var GetBeforeColumn1 = j > 1 ? ws.Cells[i, j - 1].Value : null;

                                        switch (column)
                                        {
                                            case "city":
                                                // GetBeforeColumn1 = vl.ToString();
                                                var checkCity = db.ca_places.Where(x => vl.ToString() == x.name && x.parent_id == null).FirstOrDefault();
                                                if (checkCity != null)
                                                { break; }
                                                else
                                                {
                                                    var res = db.ca_places.Where(x => x.parent_id == null).Count();
                                                    dv.parent_id = null;
                                                    dv.name = vl.ToString();
                                                    dv.status = true;
                                                    dv.is_level = 1;
                                                    dv.is_order = Convert.ToInt32(res.ToString()) + j;
                                                    dv.created_by = uid;
                                                    dv.created_date = DateTime.Now;
                                                    dv.created_ip = ip;
                                                    dv.created_token_id = tid;
                                                    db.ca_places.Add(dv);
                                                    db.SaveChanges();
                                                    break;
                                                }
                                            case "district":
                                                //     GetBeforeColumn2 = vl.ToString();
                                                var city = db.ca_places.Where(x => GetBeforeColumn1.ToString() == x.name).FirstOrDefault();
                                                city_ID = city.place_id;
                                                var checkDistrict = db.ca_places.AsNoTracking().Where(x => vl.ToString() == x.name && x.parent_id == city.place_id).FirstOrDefault();
                                                int countDistrict = db.ca_places.Where(x => x.parent_id == city.place_id).Count();
                                                if (checkDistrict != null)
                                                { break; }
                                                else
                                                {
                                                    dv.parent_id = city.place_id;
                                                    dv.name = vl.ToString();
                                                    dv.status = true;
                                                    dv.is_level = 2;
                                                    dv.is_order = countDistrict + j - 1;
                                                    dv.created_by = uid;
                                                    dv.created_date = DateTime.Now;
                                                    dv.created_ip = ip;
                                                    dv.created_token_id = tid;
                                                    db.ca_places.Add(dv);
                                                    db.SaveChanges();
                                                    break;

                                                }
                                            case "wards":
                                                if (vl != null && vl.ToString() != "")
                                                {
                                                    var district = db.ca_places.Where(x => GetBeforeColumn1.ToString() == x.name && x.parent_id == city_ID).FirstOrDefault();
                                                    var checkWards = db.ca_places.AsNoTracking().Where(x => vl.ToString() == x.name && x.parent_id == district.place_id).FirstOrDefault();
                                                    if (checkWards != null || vl == null)
                                                    { break; }
                                                    else
                                                    {
                                                        int countWards = db.ca_places.Where(x => x.parent_id == district.place_id).Count();
                                                        dv.parent_id = district.place_id;
                                                        dv.name = vl.ToString();
                                                        dv.status = true;
                                                        dv.is_level = 3;
                                                        dv.is_order = countWards + j - 2;
                                                        dv.created_by = uid;
                                                        dv.created_date = DateTime.Now;
                                                        dv.created_ip = ip;
                                                        dv.created_token_id = tid;
                                                        db.ca_places.Add(dv);
                                                        db.SaveChanges();
                                                        break;
                                                    }
                                                }
                                                else { break; }
                                        }
                                    }

                                }
                                if (dvs.Count > 0)
                                {
                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }

                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Place", ip, tid, "Lỗi khi Import Excel", 1, "ca_places");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Place", ip, tid, "Lỗi khi Import Excel", 1, "ca_place");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        } // ????????
        [HttpPost]
        public async Task<HttpResponseMessage> Import_Position()
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
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string errorCode = ""; string listErr = "";
                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");
                        FileInfo temp = new FileInfo(fpath);

                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<ca_positions> dvs = new List<ca_positions>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "TÊN CHỨC VỤ" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Column != 4) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 4].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        if (ws.Cells[i, 4].Value.ToString().ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (ws.Cells[i, 4].Value.ToString().ToUpper() != "HỆ THỐNG")
                                        {
                                            //var org = db.sys_organization.Where(x => ws.Cells[i, 4].Value.ToString().Contains(x.organization_name)).FirstOrDefault();
                                            string cellOrg = ws.Cells[i, 4].Value.ToString();
                                            var org = db.sys_organization.Where(x => cellOrg.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }
                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên chức vụ</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 500
                                  ? "Dòng thứ " + i + " cột <b>Tên chức vụ</b> không quá 500 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 4].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;
                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    ca_positions dv = new ca_positions();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.ca_positions.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "position_name":
                                                dv.position_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = Convert.ToInt32(temp_id.organization_id);
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    if (sp == true) dv.is_system = true;
                                    else dv.is_system=false;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.ca_positions.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0";
                                    listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                listErr = e.Message;
                                errorCode = "1";
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                listErr = e.StackTrace;
                                errorCode = "1";
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Position", ip, tid, "Lỗi khi Import Excel", 0, "ca_position");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Position", ip, tid, "Lỗi khi Import Excel", 0, "ca_position");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        } //done
        [HttpPost]
        public async Task<HttpResponseMessage> ImportDispatch()
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string listErr = "";
                    string errorCode = "";
                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");
                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_dispatch_books> dvs = new List<doc_ca_dispatch_books>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 12) // số cột khác 12 
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "TÊN SỔ CÔNG VĂN" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                {
                                    errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 9].Value != null &&
                                        ws.Cells[i, 9].Value.ToString().ToUpper() != "HỆ THỐNG")// kiểm tra đơn vị có trong dtb
                                    {
                                        string cell9 = ws.Cells[i, 9].Value.ToString();
                                        var org = db.sys_organization.AsNoTracking().Where(x => cell9 == x.organization_name).FirstOrDefault();
                                        if (org == null)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red'>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                        }


                                        if (ws.Cells[i, 8].Value != null && ws.Cells[i, 8].Value.ToString() != "") //kiểm tra phòng ban của đơn vị
                                        {
                                            string cell8 = ws.Cells[i, 8].Value.ToString();
                                            int id = org.organization_name == cell8 ? org.organization_id : getID(org.organization_id, cell8);
                                            if (id == 0 || id.ToString() == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Phòng ban</b>: <b style='color:red'>Đơn vị không tồn tại phòng ban " + ws.Cells[i, 8].Value + " trong hệ thống</b><br>";
                                            }
                                        }
                                    }
                                    if (ws.Cells[i, 9].Value.ToString().ToUpper() == "HỆ THỐNG" || ws.Cells[i, 9].Value.ToString().Trim() == null)
                                    { listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red'>Đơn vị không tồn tại trong hệ thống</b> <br>"; }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên khối cơ quan</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 250
                                        ? "Dòng thứ " + i + " cột <b>Tên khối cơ quan</b> không quá 250 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 4].Value == null || ws.Cells[i, 4].Value.ToString() == "" ? "Dòng thứ " + i + " cột <b>Số hiện tại</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 9].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 12].Value == null ? "Dòng thứ " + i + " cột <b>Loại văn bản</b> không được để trống <br>" : null;
                                }
                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_dispatch_books dv = new doc_ca_dispatch_books();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_dispatch_books.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j + 1].Value;
                                        var vl = ws.Cells[i, j].Value;
                                        switch (column)
                                        {
                                            case "stt":
                                                break;
                                            case "dispatch_book_name":
                                                dv.dispatch_book_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "year":
                                                if (vl != null && vl.ToString() != "")
                                                { dv.year = int.Parse(vl.ToString()); }
                                                else
                                                { dv.year = Convert.ToInt32(DateTime.Now.Year.ToString()); }
                                                break;
                                            case "current_num":
                                                if (vl != null & vl.ToString() != "")
                                                { dv.current_num = int.Parse(vl.ToString()); }
                                                else
                                                { dv.current_num = null; }
                                                break;
                                            case "tracking_place":
                                                dv.tracking_place = vl != null ? vl.ToString() : null;
                                                break;
                                            case "open_date":
                                                if (vl != null && vl.ToString() != "")
                                                {
                                                    dv.open_date = Convert.ToDateTime(vl.ToString(), new CultureInfo("en-US", true));
                                                }
                                                else
                                                {
                                                    DateTime date = DateTime.Now;
                                                    var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
                                                    dv.open_date = firstDayOfMonth;
                                                }
                                                break;
                                            case "end_date":
                                                if (vl != null && vl.ToString() != "")
                                                { dv.end_date = Convert.ToDateTime(vl.ToString(), new CultureInfo("en-US", true)); }
                                                else
                                                { dv.end_date = null; }
                                                break;
                                            case "department":
                                                if (vl != null && vl.ToString() != "")
                                                {
                                                    int d_id = 0;
                                                    var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                    if (temp_id.organization_name == vl.ToString())
                                                    { d_id = temp_id.organization_id; }
                                                    else
                                                    {
                                                        d_id = getID(temp_id.organization_id, vl.ToString());
                                                    }
                                                    if (d_id != 0)
                                                    {
                                                        dv.department_id = Convert.ToInt32(d_id);
                                                    }
                                                    else
                                                    { dv.department_id = null; }
                                                    break;
                                                }
                                                else { break; }
                                            case "organization":
                                                var o_id = db.sys_organization.Where(x => vl.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                if (o_id != null)
                                                {
                                                    dv.organization_id = Convert.ToInt32(o_id.organization_id);
                                                }
                                                else
                                                { dv.organization_id = null; }
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "is_shared":
                                                if (vl != null && vl.ToString() != "")
                                                { dv.is_shared = vl.ToString().ToUpper() == "CHIA SẺ" ? true : false; }
                                                else
                                                { dv.is_shared = false; }
                                                break;
                                            case "nav_type":
                                                if (vl != null)
                                                {
                                                    if (vl.ToString().ToUpper() == "VĂN BẢN ĐẾN")
                                                    {
                                                        dv.nav_type = 1;
                                                    }
                                                    else if (vl.ToString().ToUpper() == "VĂN BẢN ĐI")
                                                    {
                                                        dv.nav_type = 2;
                                                    }
                                                    else dv.nav_type = 3;
                                                }
                                                else
                                                { dv.nav_type = null; }
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_dispatch_books.AddRange(dvs);
                                    db.SaveChanges();
                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.ToString();
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/ImportDispatch", ip, tid, "Lỗi khi Import Excel", 0, "doc_ca_dispatch_books");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/ImportDispatch", ip, tid, "Lỗi khi Import Excel", 0, "doc_ca_dispatch_books");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" + e });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_EmailGroup()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_email_groups> dvs = new List<doc_ca_email_groups>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 5) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "TÊN NHÓM EMAIL" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 4].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        if (ws.Cells[i, 4].Value.ToString().ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (ws.Cells[i, 4].Value.ToString().ToUpper() != "HỆ THỐNG")
                                        {
                                            //string celli4 = ws.Cells[i, 4].Value.ToString();
                                            //var org = db.sys_organization.Where(x => celli4.Contains(x.organization_name)).FirstOrDefault();
                                            //if (org == null)
                                            //{
                                            //    listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            //}

                                            string celli4 = ws.Cells[i, 4].Value.ToString();
                                            var org = db.sys_organization.Where(x => celli4.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }
                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên nhóm email</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 250
                                      ? "Dòng thứ " + i + " cột <b>Tên nhóm Email</b> không quá 250 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 4].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;

                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_email_groups dv = new doc_ca_email_groups();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_email_groups.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "email_group_name":
                                                dv.email_group_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;

                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_email_groups.AddRange(dvs);
                                    db.SaveChanges();
                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_EmailGroup", ip, tid, "Lỗi khi Import Excel", 0, "email_group");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_EmailGroup", ip, tid, "Lỗi khi Import Excel", 0, "email_group");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_Email()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_emails> dvs = new List<doc_ca_emails>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 5) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "EMAIL" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    //if (ws.Cells[i, 4].Value != null) // kiểm tra đơn vị có trong dtb
                                    //{
                                    //    if (ws.Cells[i, 4].Value.ToString().ToUpper() == "HỆ THỐNG" && sp == false)
                                    //    {
                                    //        listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                    //    }
                                    //    else if (ws.Cells[i, 4].Value.ToString().ToUpper() != "HỆ THỐNG")
                                    //    {
                                    //        var org = db.sys_organization.Where(x => ws.Cells[i, 4].Value.ToString().Contains(x.organization_name)).FirstOrDefault();
                                    //        if (org == null)
                                    //        {
                                    //            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                    //        }
                                    //    }
                                    //    //if (ws.Cells[i, 8].Value != null) //kiểm tra phòng ban của đơn vị
                                    //    //{
                                    //    //    var dep = db.sys_organization.Where(x => ws.Cells[i, 8].Value.ToString().Contains(x.organization_name)
                                    //    //            && x.parent_id == org.organization_id).FirstOrDefault();
                                    //    //    if (dep == null)
                                    //    //    {
                                    //    //        listErr += "Dòng thứ " + i + " cột <b>Phòng ban</>: <b>Đơn vị không tồn tại phòng ban " + ws.Cells[i, 8].Value + "trong hệ thống</b>";
                                    //    //    }
                                    //    //}
                                    //}
                                    ////--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Email</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 50
                                      ? "Dòng thứ " + i + " cột <b>Email</b> không quá 50 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 3].Value == null ? "Dòng thứ " + i + " cột <b>Họ tên</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 3].Value != null && ws.Cells[i, 3].Value.ToString().Length > 250
                                     ? "Dòng thứ " + i + " cột <b>Họ tên</b> không quá 250 ký tự <br>" : null;
                                    //listErr += ws.Cells[i, 4].Value == null ? "Dòng thứ " + i + " cột <b>Số hiện tại</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 5].Value == null ? "Dòng thứ " + i + " cột <b>Nơi theo dõi</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 6].Value == null ? "Dòng thứ " + i + " cột <b>Ngày mở</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 7].Value == null ? "Dòng thứ " + i + " cột <b>Ngày đóng</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 8].Value == null ? "Dòng thứ " + i + " cột <b>Phòng ban</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 3].Value == null ? "Dòng thứ " + i + " cột <b>Họ tên</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 10].Value == null ? "Dòng thứ " + i + " cột <b>Hiển thị</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 11].Value == null ? "Dòng thứ " + i + " cột <b>Chia sẻ</b> không được để trống <br>" : null; 
                                    //listErr += ws.Cells[i, 12].Value == null ? "Dòng thứ " + i + " cột <b>Loại văn bản</b> không được để trống <br>" : null;
                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_emails dv = new doc_ca_emails();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_emails.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                            string id = provider.FormData.GetValues("id").SingleOrDefault();
                                            dv.email_group_id = Convert.ToInt32(id.ToString());
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "email_name":
                                                dv.email_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "full_name":
                                                dv.full_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "description":
                                                dv.description = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_emails.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Email", ip, tid, "Lỗi khi Import Excel", 0, "email");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Email", ip, tid, "Lỗi khi Import Excel", 0, "email");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_Field()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_fields> dvs = new List<doc_ca_fields>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 4) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "TÊN LĨNH VỰC" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 4].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        if (ws.Cells[i, 4].Value.ToString().ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (ws.Cells[i, 4].Value.ToString().ToUpper() != "HỆ THỐNG")
                                        {
                                            //string celli4 = ws.Cells[i, 4].Value.ToString();
                                            //var org = db.sys_organization.Where(x => celli4.Contains(x.organization_name)).FirstOrDefault();
                                            //if (org == null)
                                            //{
                                            //    listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            //}

                                            string celli4 = ws.Cells[i, 4].Value.ToString();
                                            var org = db.sys_organization.Where(x => celli4.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }

                                    }
                                    ////--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên lĩnh vực</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 250
                                      ? "Dòng thứ " + i + " cột <b>Tên lĩnh vực</b> không quá 250 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 4].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;
                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_fields dv = new doc_ca_fields();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_emails.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;

                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "field_name":
                                                dv.field_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_fields.AddRange(dvs);
                                    db.SaveChanges();
                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Field", ip, tid, "Lỗi khi Import Excel", 0, "doc_ca_field");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Field", ip, tid, "Lỗi khi Import Excel", 0, "doc_ca_field");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_CaGroup()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_groups> dvs = new List<doc_ca_groups>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 7) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "TÊN NHÓM VĂN BẢN" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 7].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        if (ws.Cells[i, 7].Value.ToString().ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (ws.Cells[i, 7].Value.ToString().ToUpper() != "HỆ THỐNG")
                                        {
                                            //string celli4 = ws.Cells[i, 7].Value.ToString();
                                            //var org = db.sys_organization.Where(x => celli4.Contains(x.organization_name)).FirstOrDefault();
                                            //if (org == null)
                                            //{
                                            //    listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            //}

                                            string celli4 = ws.Cells[i, 4].Value.ToString();
                                            var org = db.sys_organization.Where(x => celli4.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }
                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên nhóm văn bản</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 50
                                      ? "Dòng thứ " + i + " cột <b>Tên nhóm văn bản</b> không quá 50 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 3].Value == null ? "Dòng thứ " + i + " cột <b>Ký hiệu</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 3].Value != null && ws.Cells[i, 3].Value.ToString().Length > 50
                                      ? "Dòng thứ " + i + " cột <b>Ký hiệu</b> không quá 50 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 7].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;
                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_groups dv = new doc_ca_groups();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_groups.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "doc_group_name":
                                                dv.doc_group_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "doc_group_code":
                                                dv.doc_group_code = vl != null ? vl.ToString() : null;
                                                break;
                                            case "year":
                                                if (vl.ToString() != null && vl.ToString() != "")
                                                { dv.year = int.Parse(vl.ToString()); }
                                                else
                                                { dv.year = null; }
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;

                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_groups.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_CaGroup", ip, tid, "Lỗi khi Import Excel", 0, "doc_ca_group");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_CaGroup", ip, tid, "Lỗi khi Import Excel", 0, "doc_ca_group");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_IssuePlace()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_issue_places> dvs = new List<doc_ca_issue_places>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 8) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "TÊN NƠI BAN HÀNH" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }

                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 8].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        if (ws.Cells[i, 8].Value.ToString().ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (ws.Cells[i, 8].Value.ToString().ToUpper() != "HỆ THỐNG")
                                        {
                                            //string celli8 = ws.Cells[i, 8].Value.ToString();
                                            //var org = db.sys_organization.Where(x => celli8.Contains(x.organization_name)).FirstOrDefault();
                                            //if (org == null)
                                            //{
                                            //    listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            //}

                                            string celli8 = ws.Cells[i, 4].Value.ToString();
                                            var org = db.sys_organization.Where(x => celli8.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }

                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên nhóm văn bản</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 250
                                      ? "Dòng thứ " + i + " cột <b>Nơi ban hành</b> không quá 250 ký tự <br>" : null;

                                    listErr += ws.Cells[i, 8].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;

                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_issue_places dv = new doc_ca_issue_places();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_issue_places.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "issue_place_name":
                                                dv.issue_place_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "static_code":
                                                dv.static_code = vl != null ? vl.ToString() : null;
                                                break;
                                            case "dynamic_code":
                                                dv.dynamic_code = vl != null ? vl.ToString() : null;
                                                break;
                                            case "search_code":
                                                dv.search_code = vl != null ? vl.ToString() : null;
                                                break;
                                            case "display_code":
                                                dv.display_code = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;

                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_issue_places.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_IssuePlace", ip, tid, "Lỗi khi Import Excel", 0, "issue_place");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_IssuePlace", ip, tid, "Lỗi khi Import Excel", 0, "issue_place");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_Urgency()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_urgency> dvs = new List<doc_ca_urgency>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 4) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "ĐỘ KHẨN" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 4].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        if (ws.Cells[i, 4].Value.ToString().ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (ws.Cells[i, 4].Value.ToString().ToUpper() != "HỆ THỐNG")
                                        {
                                            //var org = db.sys_organization.Where(x => ws.Cells[i, 4].Value.ToString().Contains(x.organization_name)).FirstOrDefault();
                                            string cellOrg = ws.Cells[i, 4].Value.ToString();
                                            var org = db.sys_organization.Where(x => cellOrg.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }
                                        //if (ws.Cells[i, 8].Value != null) //kiểm tra phòng ban của đơn vị
                                        //{
                                        //    var dep = db.sys_organization.Where(x => ws.Cells[i, 8].Value.ToString().Contains(x.organization_name)
                                        //            && x.parent_id == org.organization_id).FirstOrDefault();
                                        //    if (dep == null)
                                        //    {
                                        //        listErr += "Dòng thứ " + i + " cột <b>Phòng ban</>: <b>Đơn vị không tồn tại phòng ban " + ws.Cells[i, 8].Value + "trong hệ thống</b>";
                                        //    }
                                        //}
                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Độ khẩn</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 250
                                      ? "Dòng thứ " + i + " cột <b>Độ khẩn</b> không quá 250 ký tự <br>" : null;
                                    //listErr += ws.Cells[i, 3].Value == null ? "Dòng thứ " + i + " cột <b>Năm</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 4].Value == null ? "Dòng thứ " + i + " cột <b>Số hiện tại</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 5].Value == null ? "Dòng thứ " + i + " cột <b>Nơi theo dõi</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 6].Value == null ? "Dòng thứ " + i + " cột <b>Ngày mở</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 7].Value == null ? "Dòng thứ " + i + " cột <b>Ngày đóng</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 8].Value == null ? "Dòng thứ " + i + " cột <b>Phòng ban</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 4].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 10].Value == null ? "Dòng thứ " + i + " cột <b>Hiển thị</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 11].Value == null ? "Dòng thứ " + i + " cột <b>Chia sẻ</b> không được để trống <br>" : null; 
                                    //listErr += ws.Cells[i, 12].Value == null ? "Dòng thứ " + i + " cột <b>Loại văn bản</b> không được để trống <br>" : null;
                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_urgency dv = new doc_ca_urgency();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_urgency.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "urgency_name":
                                                dv.urgency_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;

                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_urgency.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Urgency", ip, tid, "Lỗi khi Import Excel", 0, "urgency");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Urgency", ip, tid, "Lỗi khi Import Excel", 0, "urgency");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_Security()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_security> dvs = new List<doc_ca_security>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 4) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "ĐỘ MẬT" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 4].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        if (ws.Cells[i, 4].Value.ToString().ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (ws.Cells[i, 4].Value.ToString().ToUpper() != "HỆ THỐNG")
                                        {
                                            //var org = db.sys_organization.Where(x => ws.Cells[i, 4].Value.ToString().Contains(x.organization_name)).FirstOrDefault();
                                            string cellOrg = ws.Cells[i, 4].Value.ToString();
                                            var org = db.sys_organization.Where(x => cellOrg.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }
                                        //if (ws.Cells[i, 8].Value != null) //kiểm tra phòng ban của đơn vị
                                        //{
                                        //    var dep = db.sys_organization.Where(x => ws.Cells[i, 8].Value.ToString().Contains(x.organization_name)
                                        //            && x.parent_id == org.organization_id).FirstOrDefault();
                                        //    if (dep == null)
                                        //    {
                                        //        listErr += "Dòng thứ " + i + " cột <b>Phòng ban</>: <b>Đơn vị không tồn tại phòng ban " + ws.Cells[i, 8].Value + "trong hệ thống</b>";
                                        //    }
                                        //}
                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Độ mật</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 250
                                      ? "Dòng thứ " + i + " cột <b>Độ mật</b> không quá 250 ký tự <br>" : null;
                                    //listErr += ws.Cells[i, 3].Value == null ? "Dòng thứ " + i + " cột <b>Năm</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 4].Value == null ? "Dòng thứ " + i + " cột <b>Số hiện tại</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 5].Value == null ? "Dòng thứ " + i + " cột <b>Nơi theo dõi</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 6].Value == null ? "Dòng thứ " + i + " cột <b>Ngày mở</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 7].Value == null ? "Dòng thứ " + i + " cột <b>Ngày đóng</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 8].Value == null ? "Dòng thứ " + i + " cột <b>Phòng ban</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 4].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 10].Value == null ? "Dòng thứ " + i + " cột <b>Hiển thị</b> không được để trống <br>" : null;
                                    //listErr += ws.Cells[i, 11].Value == null ? "Dòng thứ " + i + " cột <b>Chia sẻ</b> không được để trống <br>" : null; 
                                    //listErr += ws.Cells[i, 12].Value == null ? "Dòng thứ " + i + " cột <b>Loại văn bản</b> không được để trống <br>" : null;
                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_security dv = new doc_ca_security();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_security.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "security_name":
                                                dv.security_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;

                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_security.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Security", ip, tid, "Lỗi khi Import Excel", 0, "security");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Security", ip, tid, "Lỗi khi Import Excel", 0, "Security");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_SendWay()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_send_ways> dvs = new List<doc_ca_send_ways>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 4) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "HÌNH THỨC GỬI" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 4].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        if (ws.Cells[i, 4].Value.ToString().ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (ws.Cells[i, 4].Value.ToString().ToUpper() != "HỆ THỐNG")
                                        {
                                            //var org = db.sys_organization.Where(x => ws.Cells[i, 4].Value.ToString().Contains(x.organization_name)).FirstOrDefault();
                                            string cellOrg = ws.Cells[i, 4].Value.ToString();
                                            var org = db.sys_organization.Where(x => cellOrg.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }
                                        //if (ws.Cells[i, 8].Value != null) //kiểm tra phòng ban của đơn vị
                                        //{
                                        //    var dep = db.sys_organization.Where(x => ws.Cells[i, 8].Value.ToString().Contains(x.organization_name)
                                        //            && x.parent_id == org.organization_id).FirstOrDefault();
                                        //    if (dep == null)
                                        //    {
                                        //        listErr += "Dòng thứ " + i + " cột <b>Phòng ban</>: <b>Đơn vị không tồn tại phòng ban " + ws.Cells[i, 8].Value + "trong hệ thống</b>";
                                        //    }
                                        //}
                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Hình thức gửi</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 50
                                      ? "Dòng thứ " + i + " cột <b>Hình thức gửi</b> không quá 50 ký tự <br>" : null;

                                    listErr += ws.Cells[i, 4].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;

                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_send_ways dv = new doc_ca_send_ways();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_send_ways.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "send_way_name":
                                                dv.send_way_name = vl != null ? vl.ToString() : null;
                                                break;

                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;

                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_send_ways.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_SendWay", ip, tid, "Lỗi khi Import Excel", 0, "send_way");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_SendWay", ip, tid, "Lỗi khi Import Excel", 0, "send_way");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_Signer()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_signers> dvs = new List<doc_ca_signers>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 7) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "TÊN NGƯỜI KÝ" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 5].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        //string ceilli5 = ws.Cells[i, 5].Value.ToString();
                                        //var org = db.sys_organization.Where(x => ceilli5.Contains(x.organization_name)).FirstOrDefault();
                                        //if (ws.Cells[i, 5].Value.ToString().ToUpper() == "HỆ THỐNG" && sp == false)
                                        //{
                                        //    listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        //}
                                        //else if (ws.Cells[i, 5].Value.ToString().ToUpper() != "HỆ THỐNG")
                                        //{
                                        //    if (org == null)
                                        //    {
                                        //        listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                        //    }

                                        //    if (ws.Cells[i, 4].Value != null && ws.Cells[i, 4].Value != "") //kiểm tra phòng ban của đơn vị
                                        //    {
                                        //        string cell8 = ws.Cells[i, 4].Value.ToString();
                                        //        int id = org.organization_name == cell8 ? org.organization_id : getID(org.organization_id, cell8);
                                        //        if (id == 0 || id.ToString() == null)
                                        //        {
                                        //            listErr += "Dòng thứ " + i + " cột <b>Phòng ban</b>: <b style='color:red'>Đơn vị không tồn tại phòng ban " + ws.Cells[i, 8].Value + " trong hệ thống</b><br>";
                                        //        }
                                        //    }
                                        //}
                                        string ceilli5 = ws.Cells[i, 5].Value.ToString();
                                        if (ceilli5.ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (ceilli5.ToUpper() != "HỆ THỐNG")
                                        {
                                            var org = db.sys_organization.Where(x => ceilli5.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                            if (ws.Cells[i, 4].Value != null && ws.Cells[i, 4].Value.ToString() != "") //kiểm tra phòng ban của đơn vị
                                            {
                                                string cell8 = ws.Cells[i, 4].Value.ToString();
                                                int id = org.organization_name == cell8 ? org.organization_id : getID(org.organization_id, cell8);
                                                if (id == 0 || id.ToString() == null)
                                                {
                                                    listErr += "Dòng thứ " + i + " cột <b>Phòng ban</b>: <b style='color:red'>Đơn vị không tồn tại phòng ban " + ws.Cells[i, 8].Value + " trong hệ thống</b><br>";
                                                }
                                            }
                                        }

                                        if (ws.Cells[i, 5].Value.ToString().ToUpper() == "HỆ THỐNG" || ws.Cells[i, 5].Value.ToString().Trim() == null)
                                        { listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red'>Đơn vị không tồn tại trong hệ thống</b> <br>"; }
                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên người ký</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 250
                                      ? "Dòng thứ " + i + " cột <b>Tên người ký</b> không quá 250 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 3].Value != null && ws.Cells[i, 2].Value.ToString().Length > 250
                                      ? "Dòng thứ " + i + " cột <b>Chức vụ</b> không quá 250 ký tự <br>" : null;

                                    listErr += ws.Cells[i, 5].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;

                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_signers dv = new doc_ca_signers();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_signers.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j + 1].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "signer_name":
                                                dv.signer_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "position":
                                                dv.position = vl != null ? vl.ToString() : null;
                                                break;
                                            case "department":
                                                int d_id = 0;
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                if (vl != null && vl.ToString() != "")
                                                {
                                                    if (temp_id.organization_name == vl.ToString())
                                                    { d_id = temp_id.organization_id; }
                                                    else
                                                    {
                                                        d_id = getID(temp_id.organization_id, vl.ToString());
                                                    }
                                                    if (d_id != 0)
                                                    {
                                                        dv.department_id = Convert.ToInt32(d_id);
                                                    }
                                                    else
                                                    { dv.department_id = null; }
                                                }
                                                else { dv.department_id = null; }
                                                break;
                                            case "organization":
                                                var temp_id1 = db.sys_organization.Where(x => vl.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id1.organization_id);
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "nav_type":
                                                if (vl != null)
                                                {
                                                    if (vl.ToString().ToUpper() == "VĂN BẢN ĐẾN")
                                                    {
                                                        dv.nav_type = 1;
                                                    }
                                                    else if (vl.ToString().ToUpper() == "VĂN BẢN ĐI")
                                                    {
                                                        dv.nav_type = 2;
                                                    }
                                                    else dv.nav_type = 3;
                                                }
                                                else
                                                { dv.nav_type = null; }
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_signers.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Signer", ip, tid, "Lỗi khi Import Excel", 0, "Signer");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Signer", ip, tid, "Lỗi khi Import Excel", 0, "Signer");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_ReceivePlace()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_receive_places> dvs = new List<doc_ca_receive_places>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 4) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "TÊN NƠI NHẬN" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 4].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        if (ws.Cells[i, 4].Value.ToString().ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (ws.Cells[i, 4].Value.ToString().ToUpper() != "HỆ THỐNG")
                                        {
                                            //string celli4 = ws.Cells[i, 4].Value.ToString();
                                            //var org = db.sys_organization.Where(x => celli4.Contains(x.organization_name)).FirstOrDefault();
                                            //if (org == null)
                                            //{
                                            //    listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            //}

                                            string celli4 = ws.Cells[i, 4].Value.ToString();
                                            var org = db.sys_organization.Where(x => celli4.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }
                                        //if (ws.Cells[i, 8].Value != null) //kiểm tra phòng ban của đơn vị
                                        //{
                                        //    var dep = db.sys_organization.Where(x => ws.Cells[i, 8].Value.ToString().Contains(x.organization_name)
                                        //            && x.parent_id == org.organization_id).FirstOrDefault();
                                        //    if (dep == null)
                                        //    {
                                        //        listErr += "Dòng thứ " + i + " cột <b>Phòng ban</>: <b>Đơn vị không tồn tại phòng ban " + ws.Cells[i, 8].Value + "trong hệ thống</b>";
                                        //    }
                                        //}
                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên nhóm văn bản</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 50
                                      ? "Dòng thứ " + i + " cột <b>Tên nhóm văn bản</b> không quá 50 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 4].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;
                                }
                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_receive_places dv = new doc_ca_receive_places();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_receive_places.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "receive_place_name":
                                                dv.receive_place_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_receive_places.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_ReceivePlace", ip, tid, "Lỗi khi Import Excel", 0, "Receive_place");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_ReveivePlace", ip, tid, "Lỗi khi Import Excel", 0, "ReceivePlace");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_Stamp()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_stamps> dvs = new List<doc_ca_stamps>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 6) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "TÊN TEM" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 6].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        //string celli6 = ws.Cells[i, 6].Value.ToString();
                                        //if (celli6.ToUpper() == "HỆ THỐNG" && sp == false)
                                        //{
                                        //    listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        //}
                                        //else if (celli6.ToUpper() != "HỆ THỐNG")
                                        //{
                                        //    var org = db.sys_organization.Where(x => celli6.Contains(x.organization_name)).FirstOrDefault();
                                        //    if (org == null)
                                        //    {
                                        //        listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                        //    }
                                        //}

                                        string celli6 = ws.Cells[i, 6].Value.ToString();
                                        if (celli6.ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (celli6.ToUpper() != "HỆ THỐNG")
                                        {
                                            var org = db.sys_organization.Where(x => celli6.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }
                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên tem</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 50
                                      ? "Dòng thứ " + i + " cột <b>Tên tem</b> không quá 50 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 6].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;
                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_stamps dv = new doc_ca_stamps();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_groups.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j + 1].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "stamp_name":
                                                dv.stamp_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "image":
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "is_default":
                                                dv.is_default = false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => vl.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_stamps.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Stamp", ip, tid, "Lỗi khi Import Excel", 0, "Stamp");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Stamp", ip, tid, "Lỗi khi Import Excel", 0, "Stamp");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_Status()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_status> dvs = new List<doc_ca_status>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 7) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "MÃ TRẠNG THÁI" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    string celli2 = ws.Cells[i, 2].Value.ToString();
                                    var check = db.doc_ca_status.AsNoTracking().FirstOrDefault(x => x.status_id == celli2);

                                    listErr += check != null ? "Dòng thứ " + i + " cột <b>Mã trạng thái</b> đã có trong cơ sở dữ liệu" : null;
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Mã trạng thái</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 50
                                      ? "Dòng thứ " + i + " cột <b>Mã trạng thái</b> không quá 50 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên trạng thái</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 50
                                      ? "Dòng thứ " + i + " cột <b>Tên trạng thái</b> không quá 50 ký tự <br>" : null;
                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_status dv = new doc_ca_status();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_groups.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "status_id":
                                                dv.status_id = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status_name":
                                                dv.status_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "background_color":
                                                dv.background_color = vl != null ? vl.ToString() : null;
                                                break;
                                            case "text_color":
                                                dv.text_color = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "is_handle":
                                                dv.status = vl.ToString().ToUpper() == "XỬ LÝ" ? true : false;
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_status.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Status", ip, tid, "Lỗi khi Import Excel", 0, "Status");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Status", ip, tid, "Lỗi khi Import Excel", 0, "Status");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }//done
        [HttpPost]
        public async Task<HttpResponseMessage> Import_Tags()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_tags> dvs = new List<doc_ca_tags>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 4) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "NHÃN" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    //string celli4 = ws.Cells[i, 4].Value.ToString();
                                    //if (ws.Cells[i, 4].Value != null) // kiểm tra đơn vị có trong dtb
                                    //{
                                    //    if (celli4.ToUpper() == "HỆ THỐNG" && sp == false)
                                    //    {
                                    //        listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                    //    }
                                    //    else if (celli4.ToUpper() != "HỆ THỐNG")
                                    //    {
                                    //        var org = db.sys_organization.Where(x => celli4.Contains(x.organization_name)).FirstOrDefault();
                                    //        if (org == null)
                                    //        {
                                    //            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                    //        }
                                    //    }
                                    //}
                                    if (ws.Cells[i, 4].Value != null)
                                    {
                                        string celli4 = ws.Cells[i, 4].Value.ToString();
                                        if (celli4.ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (celli4.ToUpper() != "HỆ THỐNG")
                                        {
                                            var org = db.sys_organization.Where(x => celli4.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }
                                    }

                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Nhãn</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 50
                                      ? "Dòng thứ " + i + " cột <b>Nhãn</b> không quá 50 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 4].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;
                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_tags dv = new doc_ca_tags();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_tags.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "tag_name":
                                                dv.tag_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;

                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_tags.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Tags", ip, tid, "Lỗi khi Import Excel", 0, "Tags");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Tags", ip, tid, "Lỗi khi Import Excel", 0, "Tags");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Import_Type()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_types> dvs = new List<doc_ca_types>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 6) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "MÃ LOẠI VĂN BẢN" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    //string celli2 = ws.Cells[i, 2].Value.ToString();
                                    //var check = db.doc_ca_types.Where(x => x.doc_type_code == celli2).FirstOrDefault();
                                    //listErr += check != null ? "Dòng thứ " + i + " cột <b>Mã loại</b> đã có trong cơ sở dữ liệu" : null;
                                    if (ws.Cells[i, 5].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        //string celli4 = ws.Cells[i, 5].Value.ToString();
                                        //if (celli4.ToUpper() == "HỆ THỐNG" && sp == false)
                                        //{
                                        //    listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        //}
                                        //else if (celli4.ToUpper() != "HỆ THỐNG")
                                        //{
                                        //    var org = db.sys_organization.Where(x => celli4.Contains(x.organization_name)).FirstOrDefault();
                                        //    if (org == null)
                                        //    {
                                        //        listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                        //    }
                                        //}

                                        string celli4 = ws.Cells[i, 5].Value.ToString();
                                        if (celli4.ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (celli4.ToUpper() != "HỆ THỐNG")
                                        {
                                            var org = db.sys_organization.Where(x => celli4.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }

                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Mã loại văn bản</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 50
                                      ? "Dòng thứ " + i + " cột <b>Tên nhóm văn bản</b> không quá 50 ký tự <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên loại văn bản</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 50
                                      ? "Dòng thứ " + i + " cột <b>Tên loại văn bản</b> không quá 50 ký tự <br>" : null;

                                    listErr += ws.Cells[i, 5].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;

                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_types dv = new doc_ca_types();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_types.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "doc_type_code":
                                                dv.doc_type_code = vl != null ? vl.ToString() : null;
                                                break;
                                            case "doc_type_name":
                                                dv.doc_type_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                dv.status = vl.ToString().ToUpper() == "HIỂN THỊ" ? true : false;
                                                break;
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;
                                            case "nav_type":
                                                if (vl != null)
                                                {
                                                    if (vl.ToString().ToUpper() == "VĂN BẢN ĐẾN")
                                                    {
                                                        dv.nav_type = 1;
                                                    }
                                                    else if (vl.ToString().ToUpper() == "VĂN BẢN ĐI")
                                                    {
                                                        dv.nav_type = 2;
                                                    }
                                                    else dv.nav_type = 3;
                                                }
                                                else
                                                { dv.nav_type = null; }
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_types.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Type", ip, tid, "Lỗi khi Import Excel", 0, "Type");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Type", ip, tid, "Lỗi khi Import Excel", 0, "Type");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> Import_CaPosition()
        {
            string fpath = "";
            using (DBEntities db = new DBEntities())
            {
                string listErr = "";
                string errorCode = "";
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }

                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));

                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");

                        FileInfo temp = new FileInfo(fpath);
                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<doc_ca_positions> dvs = new List<doc_ca_positions>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                if (ws.Dimension.End.Column != 5) // số cột khác x (x phụ thuộc vào mẫu excel)
                                {
                                    errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }

                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 1; i <= ws.Dimension.Columns; i++)
                                {
                                    if (col_name.ToUpper() == "TÊN CHỨC VỤ VĂN BẢN" && ws.Cells[3, i].Value != null) // check case
                                    {
                                        errorCode = "0";
                                        listErr = "";
                                    }
                                    else
                                    {
                                        errorCode = "1"; listErr = "Sai mẫu Excel!<br> Vui lòng kiểm tra lại!";
                                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                    }
                                }
                                if (ws.Dimension.End.Row <= 4) // số dòng ít hơn 5
                                { errorCode = "1"; listErr = "Không có dữ liệu!<br> Vui lòng kiểm tra lại!"; return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr }); }

                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 5].Value != null) // kiểm tra đơn vị có trong dtb
                                    {
                                        //string celli5 = ws.Cells[i, 5].Value.ToString();
                                        //if (celli5.ToUpper() == "HỆ THỐNG" && sp == false)
                                        //{
                                        //    listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        //}
                                        //else if (celli5.ToUpper() != "HỆ THỐNG")
                                        //{
                                        //    var org = db.sys_organization.Where(x => celli5.Contains(x.organization_name)).FirstOrDefault();
                                        //    if (org == null)
                                        //    {
                                        //        listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                        //    }
                                        //}

                                        string celli5 = ws.Cells[i, 5].Value.ToString();
                                        if (celli5.ToUpper() == "HỆ THỐNG" && sp == false)
                                        {
                                            listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b style='color:red;'>Bạn không phải quản trị viên hệ thống</b> <br>";
                                        }
                                        else if (celli5.ToUpper() != "HỆ THỐNG")
                                        {
                                            var org = db.sys_organization.Where(x => celli5.ToLower().Contains(x.organization_name.Trim().ToLower())).FirstOrDefault();
                                            if (org == null)
                                            {
                                                listErr += "Dòng thứ " + i + " cột <b>Đơn vị</b>: <b>Đơn vị không tồn tại trong hệ thống</b> <br>";
                                            }
                                        }
                                    }
                                    //--------------------------------Kiểm tra trường bắt buộc nhập-----------------------------------
                                    listErr += ws.Cells[i, 1].Value == null ? "Dòng thứ " + i + " cột <b>STT</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value == null ? "Dòng thứ " + i + " cột <b>Tên chức vụ văn bản</b> không được để trống <br>" : null;
                                    listErr += ws.Cells[i, 2].Value != null && ws.Cells[i, 2].Value.ToString().Length > 250
                                      ? "Dòng thứ " + i + " cột <b>Tên chức vụ văn bản</b> không quá 250 ký tự <br>" : null;

                                    listErr += ws.Cells[i, 5].Value == null ? "Dòng thứ " + i + " cột <b>Đơn vị</b> không được để trống <br>" : null;

                                }

                                if (listErr.Length > 0) // kiểm tra đúng sai file nhập vào-> sai báo lỗi
                                {
                                    errorCode = "1";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });//dừng code và báo lỗi ra mh
                                }
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    doc_ca_positions dv = new doc_ca_positions();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.doc_ca_positions.Count();
                                            dv.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "position_name":
                                                dv.position_name = vl != null ? vl.ToString() : null;
                                                break;
                                            case "status":
                                                if (vl != null && vl.ToString() != "")
                                                {
                                                    string des = vl.ToString().ToUpper().TrimEnd();
                                                    dv.status = des == "HIỂN THỊ" ? true : false;
                                                    break;
                                                }
                                                else
                                                {
                                                    dv.status = false;
                                                    break;
                                                }
                                            case "organization":
                                                var temp_id = db.sys_organization.Where(x => GetNameOrg.ToString().Contains(x.organization_name)).FirstOrDefault();
                                                dv.organization_id = vl.ToString().ToUpper() == "HỆ THỐNG" ? 0 : Convert.ToInt32(temp_id.organization_id);
                                                break;
                                            case "p_type":
                                                if (vl != null)
                                                {
                                                    if (vl.ToString().ToUpper() == "TẤT CẢ")
                                                    {
                                                        dv.position_type = 0;
                                                    }
                                                    else if (vl.ToString().ToUpper() == "CÔNG TY")
                                                    {
                                                        dv.position_type = 1;
                                                    }
                                                    else if (vl.ToString().ToUpper() == "NHÀ NƯỚC") { dv.position_type = 2; }
                                                }
                                                else
                                                { dv.position_type = null; }
                                                break;
                                        }
                                    }
                                    dv.created_by = uid;
                                    dv.created_date = DateTime.Now;
                                    dv.created_ip = ip;
                                    dv.created_token_id = tid;
                                    dvs.Add(dv);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.doc_ca_positions.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0"; listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                errorCode = "1";
                                listErr = e.Message;
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_CaPosition", ip, tid, "Lỗi khi Import Excel", 0, "Ca_position");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_CaPosition", ip, tid, "Lỗi khi Import Excel", 0, "Ca_position");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        }
        #endregion
        #region Hệ thống
        [HttpPost]
        public async Task<HttpResponseMessage> Import_User()
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
                bool sp = claims.Where(p => p.Type == "super").FirstOrDefault()?.Value == "True";
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
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
                    if (!Request.Content.IsMimeMultipartContent())
                    {
                        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                    }
                    string errorCode = ""; string listErr = "";
                    string strPath = HttpContext.Current.Server.MapPath("~/Portals/Excel/");
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
                        var listPath = fileNameTemp.Split('/');
                        var pathConfigTemp = "";
                        var sttPartPath = 1;
                        foreach (var item in listPath)
                        {
                            if (item.Trim() != "")
                            {
                                if (sttPartPath == 1)
                                {
                                    pathConfigTemp += (item);
                                }
                                else
                                {
                                    pathConfigTemp += "/" + Path.GetFileName(item);
                                }
                            }
                            sttPartPath++;
                        }
                        File.Move(pathConfigTemp, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        //File.Move(finfo.FullName, Path.Combine(strPath, guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "")));
                        fpath = strPath + guid + "_" + provider.FileData.First().Headers.ContentDisposition.FileName.Replace("\"", "");
                        FileInfo temp = new FileInfo(fpath);

                        using (ExcelPackage pck = new ExcelPackage(temp))
                        {
                            try
                            {
                                List<sys_users> dvs = new List<sys_users>();
                                ExcelWorksheet ws = pck.Workbook.Worksheets.First();
                                List<string> cols = new List<string>();
                                string col_name = ws.Cells[4, 2].Value.ToString();
                                for (int i = 5; i <= ws.Dimension.End.Row; i++)
                                {
                                    if (ws.Cells[i, 1].Value == null)
                                    {
                                        break;
                                    }
                                    sys_users user = new sys_users();

                                    for (int j = 1; j <= ws.Dimension.End.Column; j++)
                                    {
                                        if (ws.Cells[3, j].Value != null)
                                        {
                                            var res = db.sys_users.Count();
                                            user.is_order = Convert.ToInt32(res.ToString()) + i - 4;
                                        }
                                        else
                                        { break; }
                                        var column = ws.Cells[3, j].Value;
                                        var GetNameOrg = ws.Cells[i, j].Value;
                                        var vl = ws.Cells[i, j].Value;

                                        switch (column)
                                        {
                                            case "STT":
                                                break;
                                            case "user_id":
                                                user.user_id = vl != null ? vl.ToString() : null;
                                                break;
                                            case "is_psword":
                                                //string depass = vl != null ? Codec.EncryptString(vl.ToString(), helper.psKey) : null;
                                                string depass = vl != null ? BCrypt.Net.BCrypt.HashPassword(vl.ToString()) : null;
                                                user.is_psword = depass;
                                                break;
                                            case "full_name":
                                                user.full_name = vl != null ? vl.ToString() : null;
                                                user.full_name_en = vl != null ? helper.convertToUnSign(vl.ToString()) : null;
                                                if (user.full_name != null)
                                                {
                                                    var splitName = user.full_name.Split(' ');
                                                    var lastName = "";
                                                    foreach (var itemName in splitName)
                                                    {
                                                        if (itemName.Trim() != "")
                                                        {
                                                            lastName = itemName.Trim();
                                                        }
                                                    }
                                                    user.last_name = lastName != "" ? lastName : null;
                                                }
                                                else
                                                {
                                                    user.last_name = null;
                                                }
                                                //user.last_name = user.full_name_en != null && vl.ToString() != null && vl.ToString().Split(' ').Count() > 0 ? vl.ToString().Split(' ').LastOrDefault().Trim() : null;
                                                break;
                                            case "gender":
                                                user.gender = helper.convertToUnSign(vl.ToString().Trim().ToUpper()) == "NAM" ? 1 : (helper.convertToUnSign(vl.ToString().Trim().ToUpper()) == "NU" ? 0 : 2);
                                                break;
                                            case "phone":
                                                user.phone = vl != null ? vl.ToString() : null;
                                                break;
                                            case "email":
                                                user.email = vl != null ? vl.ToString() : null;
                                                break;
                                            case "birthday":
                                                if (vl != null)
                                                {
                                                    var dates = vl.ToString().Split('/');
                                                    var dd = int.Parse(dates[0]);
                                                    var mm = int.Parse(dates[1]);
                                                    var yyyy = int.Parse(dates[2]);
                                                    //user.birthday = DateTime.Parse(vl.ToString());
                                                    user.birthday = new DateTime(yyyy, mm, dd);
                                                }
                                                else user.birthday = null;
                                                break;
                                            case "status":
                                                user.status = (int)(helper.convertToUnSign(vl.ToString().Trim().ToUpper()) == "KHOA" ? 0 : helper.convertToUnSign(vl.ToString().Trim().ToUpper()) == "DOI XAC THUC" ? 2 : helper.convertToUnSign(vl.ToString().Trim().ToUpper()) == "KICH HOAT" ? 1 : (int?)null);
                                                break;
                                            case "role_id":
                                                bool check_role = (vl != null && db.sys_roles.FirstOrDefault(x => x.role_id == vl.ToString()) != null) ? true : false;
                                                user.role_id = check_role == true ? vl.ToString() : null;
                                                break;
                                            case "position_id":
                                                bool check_position = (vl != null && db.ca_positions.AsEnumerable().FirstOrDefault(x => x.position_id == int.Parse(vl.ToString())) != null) ? true : false;
                                                user.position_id = check_position == true ? int.Parse(vl.ToString()) : (int?)null;
                                                break;
                                            case "organization_id":
                                                bool check_organization = (vl != null && db.sys_organization.AsEnumerable().FirstOrDefault(x => x.organization_id == int.Parse(vl.ToString())) != null) ? true : false;
                                                user.organization_id = check_organization == true ? int.Parse(vl.ToString()) : int.Parse(dvid);
                                                break;
                                            case "department_id":
                                                bool check_department = (vl != null && db.sys_organization.AsEnumerable().FirstOrDefault(x => x.organization_id == int.Parse(vl.ToString())) != null) ? true : false;
                                                user.department_id = check_department == true ? int.Parse(vl.ToString()) : int.Parse(dvid);
                                                break;
                                        }
                                    }
                                    if (db.sys_users.Where(a => a.user_id == user.user_id).Count() > 0)
                                    {
                                        continue;
                                    }
                                    user.is_booking = false;
                                    user.wrong_pass_count = 0;
                                    user.key_encript = Convert.ToBase64String(Encoding.UTF8.GetBytes(helper.psKey));
                                    user.display_birthday = true;
                                    user.created_by = uid;
                                    user.created_date = DateTime.Now;
                                    user.created_ip = ip;
                                    user.ip = ip;
                                    user.created_token_id = tid;
                                    dvs.Add(user);
                                }
                                if (dvs.Count > 0)
                                {
                                    db.sys_users.AddRange(dvs);
                                    db.SaveChanges();

                                    errorCode = "0";
                                    listErr = "";
                                    return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr, count = dvs.Count });
                                }
                                bool exists1 = File.Exists(fpath);
                                if (exists1)
                                {
                                    System.IO.File.Delete(fpath);
                                }
                                bool exists2 = File.Exists(pathConfigTemp);
                                if (exists2)
                                {
                                    System.IO.File.Delete(pathConfigTemp);
                                }
                            }
                            catch (DbEntityValidationException e)
                            {
                                listErr = e.Message;
                                errorCode = "1";
                                string contents = helper.getCatchError(e, null);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }
                            catch (Exception e)
                            {
                                listErr = e.StackTrace;
                                errorCode = "1";
                                string contents = helper.ExceptionMessage(e);
                                if (!helper.debug)
                                {
                                    contents = "";
                                }
                                Log.Error(contents);
                                return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                            }

                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { err = errorCode, ms = listErr });
                    });
                    return await task;

                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Position", ip, tid, "Lỗi khi Import Excel", 0, "ca_position");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = fpath, contents }), domainurl + "/ImportExcel/Import_Position", ip, tid, "Lỗi khi Import Excel", 0, "ca_position");
                    if (!helper.debug)
                    {
                        contents = "";
                    }
                    Log.Error(contents);
                    return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
                }
            }
        } //done
        #endregion
    }
}
