using API.Helper;
using API.Models;
using GemBox.Document;
using GleamTech.DocumentUltimate;
using Helper;
using Microsoft.ApplicationBlocks.Data;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using OpenHtmlToPdf;
using Spire.Doc;
using Spire.Doc.Documents;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Controllers
{
    [Authorize(Roles = "login")]
    public class ExcelController : ApiController
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
        public async Task<HttpResponseMessage> ExportExcel_API_Table([System.Web.Mvc.Bind(Include = "excelname,par,proc")][FromBody] sqlProc proc)
        {
            using (DBEntities db = new DBEntities())
            {
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
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
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    string Connection = db.Database.Connection.ConnectionString;
                    using (ExcelPackage exportPackge = new ExcelPackage())
                    {
                        String FileName = String.Empty;
                        FileName = proc.excelname + "_" + DateTime.Now.ToString("ddMMyyyy");
                        #region tbody
                        var sqlpas = new List<SqlParameter>();
                        if (proc != null && proc.par != null)
                        {
                            foreach (sqlPar p in proc.par)
                            {
                                sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                            }
                        }
                        var arrpas = sqlpas.ToArray();
                        var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables);
                        var table = await task;
                        if (table[2].Rows.Count > 0)
                        {
                            var listGroup = new List<string>();
                            var hasNullGroup = false;
                            if (table[2].Rows.Count > 1)
                            {
                                for (int idxGrp = 0; idxGrp < table[2].Rows.Count; idxGrp++)
                                {
                                    if (table[2].Rows[idxGrp]["group_name"] != null && table[2].Rows[idxGrp]["group_name"].ToString() != "")
                                    {
                                        listGroup.Add(table[2].Rows[idxGrp]["group_name"].ToString());
                                    }
                                    else
                                    {
                                        hasNullGroup = true;
                                    }
                                }
                                if (hasNullGroup == true)
                                {
                                    listGroup.Add("Khác");
                                }
                            }
                            else
                            {
                                listGroup.Add("DANH SÁCH CÁC TRƯỜNG DỮ LIỆU");
                            }
                            var dateTimeExport = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
                            for (int tblGroup = 0; tblGroup < listGroup.Count(); tblGroup++)
                            {
                                ExcelWorksheet exWorkSheet = exportPackge.Workbook.Worksheets.Add(listGroup[tblGroup]);
                                #region thead
                                var len = table[0].Columns.Count - 2;
                                int start = 3;
                                ExcelRange topcell1 = exWorkSheet.Cells[1, 1, 1, len];
                                exWorkSheet.Row(1).Height = 40;
                                exWorkSheet.Row(2).Height = 20;
                                exWorkSheet.Row(3).Height = 25;
                                exWorkSheet.Row(4).Height = 25;
                                topcell1.Style.Font.Bold = true;
                                topcell1.Style.Font.Size = 16;
                                topcell1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                topcell1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                topcell1.Merge = true;
                                topcell1.Style.WrapText = true;
                                topcell1.Value = proc.excelname;
                                ExcelRange topcell2 = exWorkSheet.Cells[2, 1, 2, len];
                                topcell2.Value = dateTimeExport;
                                topcell2.Style.Font.Italic = true;
                                topcell2.Style.Font.Bold = false;
                                topcell2.Style.Font.Size = 12;
                                topcell2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                topcell2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                topcell2.Merge = true;
                                topcell2.Style.WrapText = true;
                                for (int i = 1; i <= len; i++)
                                {
                                    var col = table[0].Columns[i];
                                    ExcelRange cell = exWorkSheet.Cells[start, i];
                                    cell.Value = col.ColumnName.Split('|')[0];
                                    exWorkSheet.Row(start).Hidden = true;
                                    exWorkSheet.Row(start).Style.Font.Color.SetColor(System.Drawing.Color.Transparent);
                                }
                                start++;
                                var isGroupNull = false;
                                if (listGroup.Count == 1)
                                {
                                    isGroupNull = table[1].Rows[0]["group_name"] != null && table[1].Rows[0]["group_name"].ToString() != "" ? false : true;
                                }
                                var listTableGroup = listGroup[tblGroup] != "Khác" && listGroup.Count > 1
                                    ? table[1].AsEnumerable().Where(myRow => myRow.Field<string>("group_name") == listGroup[tblGroup]).ToList()
                                    : (listGroup[tblGroup] != "Khác" && listGroup.Count == 1 && !isGroupNull
                                        ? table[1].AsEnumerable().Where(myRow => myRow.Field<string>("group_name") == table[1].Rows[0]["group_name"].ToString()).ToList()
                                        : table[1].AsEnumerable().Where(myRow => myRow.Field<string>("group_name") == null || myRow.Field<string>("group_name") == "").ToList()
                                    );

                                for (int idxTable = 0; idxTable < listTableGroup.Count; idxTable++)
                                {
                                    var idTable = Int32.Parse(listTableGroup[idxTable]["table_id"].ToString());
                                    var nameTable = listTableGroup[idxTable]["table_name"].ToString();
                                    var motaTable = listTableGroup[idxTable]["des"].ToString();
                                    var listColTable = table[0].AsEnumerable().Where(myRow => myRow.Field<string>("table_name") == nameTable).ToList();
                                    ExcelRange topcellTbl = exWorkSheet.Cells[start, 1, start, len];
                                    topcellTbl.Value = nameTable + " (Mô tả: " + motaTable + " - Mã bảng: " + idTable + " - Số cột: " + listColTable.Count + ")";
                                    topcellTbl.Style.Font.Bold = true;
                                    topcellTbl.Style.Font.Size = 14;
                                    topcellTbl.Style.WrapText = true;
                                    topcellTbl.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    topcellTbl.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    topcellTbl.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    topcellTbl.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    topcellTbl.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    topcellTbl.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    topcellTbl.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    topcellTbl.Merge = true;
                                    topcellTbl.Style.WrapText = true;
                                    exWorkSheet.Cells[start, 1, start, len].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    exWorkSheet.Cells[start, 1, start, len].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#0078d4"));
                                    exWorkSheet.Cells[start, 1, start, len].Style.Font.Color.SetColor(System.Drawing.Color.White);
                                    start++;
                                    for (int i = 1; i <= len; i++)
                                    {
                                        var col = table[0].Columns[i];
                                        ExcelRange cell = exWorkSheet.Cells[start, i];
                                        cell.Value = col.ColumnName.Split('|')[1];
                                        cell.Style.Indent = 1;
                                        cell.Style.Font.Bold = true;
                                        exWorkSheet.Column(i).Style.Font.Size = 12;
                                        exWorkSheet.Column(i).Style.WrapText = true;
                                        exWorkSheet.Column(i).Width = int.Parse(col.ColumnName.Split('|')[2]);
                                        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                        cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                        cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                        cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                        cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3].ToUpper() == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Left;
                                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    }
                                    exWorkSheet.Cells[start, 1, start, len].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    exWorkSheet.Cells[start, 1, start, len].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#0078d4"));
                                    exWorkSheet.Cells[start, 1, start, len].Style.Font.Color.SetColor(System.Drawing.Color.White);
                                    #endregion
                                    //topcell1.Value = string.Format(proc.excelname + " ({0})", table[0].Rows.Count);
                                    for (int k = 0; k < listColTable.Count; k++)
                                    {
                                        start += 1;
                                        var dr = listColTable[k];
                                        for (int i = 1; i <= len; i++)
                                        {
                                            var col = table[0].Columns[i];
                                            ExcelRange cell = exWorkSheet.Cells[start, i];
                                            try
                                            {
                                                var contentColumn = dr[col.ColumnName].ToString();
                                                cell.Value = col.ColumnName == "col_type|Kiểu|30|l" ? (contentColumn.Replace("(-1)", "(MAX)")) : contentColumn;
                                                cell.Style.Indent = 1;
                                                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                                cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3].ToUpper() == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Left;
                                                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                            }
                                            catch (Exception e) { }
                                        }
                                    }
                                    start += 2;
                                }

                            }
                        }
                        #endregion
                        Byte[] fileBytes = exportPackge.GetAsByteArray();
                        string fname = helper.convertToUnSign3(proc.excelname.ToLower() + "_" + uid);
                        var strPath = HttpContext.Current.Server.MapPath("~/Portals/Export");
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);
                        string path = HttpContext.Current.Server.MapPath("~/Portals/Export/" + fname + ".xlsx");
                        File.WriteAllBytes(path, fileBytes);
                        return Request.CreateResponse(HttpStatusCode.OK, new { path = "/Portals/Export/" + fname + ".xlsx", err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel_API_Table", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel_API_Table", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
        public async Task<HttpResponseMessage> ExportExcel([System.Web.Mvc.Bind(Include = "excelname,par,proc")][FromBody] sqlProc proc)
        {
            using (DBEntities db = new DBEntities())
            {
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
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
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                string dvid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    string Connection = db.Database.Connection.ConnectionString;
                    using (ExcelPackage exportPackge = new ExcelPackage())
                    {
                        String FileName = String.Empty;
                        FileName = proc.excelname + "_" + DateTime.Now.ToString("ddMMyyyy");
                        ExcelWorksheet exWorkSheet = exportPackge.Workbook.Worksheets.Add(FileName);
                        #region tbody
                        var sqlpas = new List<SqlParameter>();
                        if (proc != null && proc.par != null)
                        {
                            foreach (sqlPar p in proc.par)
                            {
                                sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                            }
                        }
                        var arrpas = sqlpas.ToArray();
                        var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables[0]);
                        var table = await task;
                        #region thead
                        var len = table.Columns.Count;
                        int start = 3;
                        ExcelRange topcell1 = exWorkSheet.Cells[1, 1, 1, len];
                        exWorkSheet.Row(1).Height = 40;
                        exWorkSheet.Row(2).Height = 20;
                        exWorkSheet.Row(3).Height = 25;
                        exWorkSheet.Row(4).Height = 25;
                        topcell1.Style.Font.Bold = true;
                        topcell1.Style.Font.Size = 16;
                        topcell1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        topcell1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        topcell1.Merge = true;
                        topcell1.Style.WrapText = true;
                        ExcelRange topcell2 = exWorkSheet.Cells[2, 1, 2, len];
                        topcell2.Value = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
                        topcell2.Style.Font.Italic = true;
                        topcell2.Style.Font.Bold = false;
                        topcell2.Style.Font.Size = 12;
                        topcell2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        topcell2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        topcell2.Merge = true;
                        topcell2.Style.WrapText = true;
                        for (int i = 1; i <= len; i++)
                        {
                            var col = table.Columns[i - 1];
                            ExcelRange cell = exWorkSheet.Cells[start, i];
                            cell.Value = col.ColumnName.Split('|')[0];
                            exWorkSheet.Row(start).Hidden = true;
                            exWorkSheet.Row(start).Style.Font.Color.SetColor(System.Drawing.Color.Transparent);
                        }
                        start++;
                        for (int i = 1; i <= len; i++)
                        {
                            var col = table.Columns[i - 1];
                            ExcelRange cell = exWorkSheet.Cells[start, i];
                            cell.Value = col.ColumnName.Split('|')[1];
                            cell.Style.Indent = 1;
                            cell.Style.Font.Bold = true;
                            exWorkSheet.Column(i).Style.Font.Size = 12;
                            exWorkSheet.Column(i).Style.WrapText = true;
                            exWorkSheet.Column(i).Width = int.Parse(col.ColumnName.Split('|')[2]);
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3].ToUpper() == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        }
                        exWorkSheet.Cells[start, 1, start, len].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        exWorkSheet.Cells[start, 1, start, len].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#0078d4"));
                        exWorkSheet.Cells[start, 1, start, len].Style.Font.Color.SetColor(System.Drawing.Color.White);
                        #endregion
                        topcell1.Value = string.Format(proc.excelname + " ({0})", table.Rows.Count);
                        for (int k = 0; k < table.Rows.Count; k++)
                        {
                            start += 1;
                            var dr = table.Rows[k];
                            for (int i = 1; i <= len; i++)
                            {
                                var col = table.Columns[i - 1];
                                ExcelRange cell = exWorkSheet.Cells[start, i];
                                ExcelRange cellMauNen = exWorkSheet.Cells[4, 4];
                                ExcelRange cellMauChu = exWorkSheet.Cells[4, 5];
                                try
                                {
                                    if (dr[col.ColumnName].ToString().Contains("Portal") == true && proc.excelname == "DANH SÁCH TEM")
                                    {
                                        exWorkSheet.Row(i + 2).Height = 150;
                                        // cell.Value = dr[col.ColumnName].ToString() + "1";
                                        string strPath11 = HttpContext.Current.Server.MapPath(dr[col.ColumnName].ToString());
                                        System.Web.UI.WebControls.Image TEST_IMAGE = new System.Web.UI.WebControls.Image();

                                        var url = dr[col.ColumnName] != null ? dr[col.ColumnName].ToString() : "";
                                        url = Regex.Replace(url.Replace("\\", "/"), @"\.*/+", "/");
                                        var listPath = url.Split('/');
                                        var pathFile = "";
                                        foreach (var item in listPath)
                                        {
                                            if (item.Trim() != "")
                                            {
                                                pathFile += "/" + Path.GetFileName(item);
                                            }
                                        }
                                        if (!File.Exists(HttpContext.Current.Server.MapPath("~/") + pathFile))
                                        //string pathFileInExport = HttpContext.Current.Server.MapPath("~/Portals");
                                        //if (dr[col.ColumnName].ToString().Contains("Emote"))
                                        //{
                                        //    pathFileInExport += "/Emote";
                                        //}
                                        //if (!File.Exists(Path.Combine(pathFileInExport, Path.GetFileName(strPath11))))
                                        {
                                            cell.Value = dr[col.ColumnName].ToString() + " (File đã bị xóa hoặc chuyển đi nơi khác!)";
                                        }
                                        else
                                        {
                                            System.Drawing.Image myImage = System.Drawing.Image.FromFile(strPath11);
                                            var pic = exWorkSheet.Drawings.AddPicture(dr[col.ColumnName].ToString(), myImage);
                                            pic.SetSize(180, 180);
                                            // Row, RowoffsetPixel, Column, ColumnOffSetPixel
                                            pic.SetPosition(i + 1, 10, 2, 12);
                                        }
                                    }
                                    else
                                    {
                                        cell.Value = dr[col.ColumnName].ToString();
                                    }
                                    cell.Style.Indent = 1;
                                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3].ToUpper() == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Left;
                                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                }
                                catch (Exception e) { }
                            }
                        }
                        #endregion
                        Byte[] fileBytes = exportPackge.GetAsByteArray();
                        string fname = helper.convertToUnSign3(proc.excelname.ToLower() + "_" + uid);
                        var strPath = HttpContext.Current.Server.MapPath("~/Portals/Export");
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);
                        string path = HttpContext.Current.Server.MapPath("~/Portals/Export/" + fname + ".xlsx");
                        File.WriteAllBytes(path, fileBytes);
                        return Request.CreateResponse(HttpStatusCode.OK, new { path = "/Portals/Export/" + fname + ".xlsx", err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
        public async Task<HttpResponseMessage> ExportExcelTable([System.Web.Mvc.Bind(Include = "excelname,par,proc")][FromBody] sqlProc proc)
        {
            using (DBEntities db = new DBEntities())
            {
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
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
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    string Connection = db.Database.Connection.ConnectionString;
                    using (ExcelPackage exportPackge = new ExcelPackage())
                    {
                        String FileName = String.Empty;
                        FileName = proc.excelname + "_" + DateTime.Now.ToString("ddMMyyyy");
                        ExcelWorksheet exWorkSheet = exportPackge.Workbook.Worksheets.Add(FileName);
                        #region tbody
                        var sqlpas = new List<SqlParameter>();
                        if (proc != null && proc.par != null)
                        {
                            foreach (sqlPar p in proc.par)
                            {
                                sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                            }
                        }
                        var arrpas = sqlpas.ToArray();
                        var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables[0]);
                        var table = await task;
                        #region thead
                        var len = table.Columns.Count;
                        int start = 3;
                        ExcelRange topcell1 = exWorkSheet.Cells[1, 1, 1, len];
                        exWorkSheet.Row(1).Height = 50;
                        exWorkSheet.Row(2).Height = 20;
                        exWorkSheet.Row(3).Height = 25;
                        exWorkSheet.Row(4).Height = 25;
                        topcell1.Style.Font.Bold = true;
                        topcell1.Style.Font.Size = 16;
                        topcell1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        topcell1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        topcell1.Merge = true;
                        topcell1.Style.WrapText = true;
                        ExcelRange topcell2 = exWorkSheet.Cells[2, 1, 2, len];
                        topcell2.Value = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
                        topcell2.Style.Font.Italic = true;
                        topcell2.Style.Font.Bold = false;
                        topcell2.Style.Font.Size = 12;
                        topcell2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        topcell2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        topcell2.Merge = true;
                        topcell2.Style.WrapText = true;
                        for (int i = 1; i <= len; i++)
                        {
                            var col = table.Columns[i - 1];
                            ExcelRange cell = exWorkSheet.Cells[start, i];
                            cell.Value = col.ColumnName.Split('|')[0];
                            exWorkSheet.Row(start).Hidden = true;
                            exWorkSheet.Row(start).Style.Font.Color.SetColor(System.Drawing.Color.Transparent);
                        }
                        start++;
                        for (int i = 1; i <= len; i++)
                        {
                            var col = table.Columns[i - 1];
                            ExcelRange cell = exWorkSheet.Cells[start, i];
                            cell.Value = col.ColumnName.Split('|')[1];
                            cell.Style.Indent = 1;
                            cell.Style.Font.Bold = true;
                            exWorkSheet.Column(i).Style.Font.Size = 12;
                            exWorkSheet.Column(i).Style.WrapText = true;
                            exWorkSheet.Column(i).Width = int.Parse(col.ColumnName.Split('|')[2]);
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3].ToUpper() == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Left;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        }
                        exWorkSheet.Cells[start, 1, start, len].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        exWorkSheet.Cells[start, 1, start, len].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#0078d4"));
                        exWorkSheet.Cells[start, 1, start, len].Style.Font.Color.SetColor(System.Drawing.Color.White);
                        #endregion
                        topcell1.Value = string.Format(proc.excelname + " ({0})", table.Rows.Count);
                        for (int k = 0; k < table.Rows.Count; k++)
                        {
                            start += 1;
                            var dr = table.Rows[k];
                            for (int i = 1; i <= len; i++)
                            {
                                var col = table.Columns[i - 1];
                                ExcelRange cell = exWorkSheet.Cells[start, i];
                                try
                                {
                                    cell.Value = dr[col.ColumnName].ToString();
                                    cell.Style.Indent = 1;
                                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3].ToUpper() == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Left;
                                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                }
                                catch (Exception e) { }
                            }
                            if (k > 0 && table.Rows[k - 1].ItemArray[1].ToString() != dr.ItemArray[1].ToString())
                            {
                                exWorkSheet.Cells[start, 1, start, len].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                exWorkSheet.Cells[start, 1, start, len].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#689f38"));
                                exWorkSheet.Cells[start, 1, start, len].Style.Font.Color.SetColor(System.Drawing.Color.White);
                            }
                        }
                        #endregion
                        Byte[] fileBytes = exportPackge.GetAsByteArray();
                        string fname = helper.convertToUnSign3(proc.excelname + "_" + uid);
                        string path = HttpContext.Current.Server.MapPath("~/Portals/Export/" + fname + ".xlsx");
                        File.WriteAllBytes(path, fileBytes);
                        return Request.CreateResponse(HttpStatusCode.OK, new { path = "/Portals/Export/" + fname + ".xlsx", err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
        public async Task<HttpResponseMessage> ExportExcelMau([System.Web.Mvc.Bind(Include = "excelname,par")][FromBody] sqlProc proc)
        {
            using (DBEntities db = new DBEntities())
            {
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
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
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    string Connection = db.Database.Connection.ConnectionString;
                    using (ExcelPackage exportPackge = new ExcelPackage())
                    {
                        String FileName = String.Empty;
                        FileName = "ExelUserMau_" + DateTime.Now.ToString("ddMMyyyy");
                        ExcelWorksheet exWorkSheet = exportPackge.Workbook.Worksheets.Add(FileName);
                        #region tbody
                        var sqlpas = new List<SqlParameter>();
                        if (proc != null && proc.par != null)
                        {
                            foreach (sqlPar p in proc.par)
                            {
                                sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                            }
                        }
                        var arrpas = sqlpas.ToArray();
                        var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables[0]);
                        var table = await task;
                        #region thead
                        var len = table.Columns.Count;
                        int start = 3;
                        ExcelRange topcell1 = exWorkSheet.Cells[1, 1, 1, len];
                        exWorkSheet.Row(1).Height = 50;
                        exWorkSheet.Row(2).Height = 20;
                        exWorkSheet.Row(3).Height = 25;
                        exWorkSheet.Row(4).Height = 25;
                        topcell1.Style.Font.Bold = true;
                        topcell1.Style.Font.Size = 16;
                        topcell1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        topcell1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        topcell1.Merge = true;
                        topcell1.Style.WrapText = true;
                        ExcelRange topcell2 = exWorkSheet.Cells[2, 1, 2, len];
                        topcell2.Value = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
                        topcell2.Style.Font.Italic = true;
                        topcell2.Style.Font.Bold = false;
                        topcell2.Style.Font.Size = 12;
                        topcell2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        topcell2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        topcell2.Merge = true;
                        topcell2.Style.WrapText = true;
                        for (int i = 1; i <= len; i++)
                        {
                            var col = table.Columns[i - 1];
                            ExcelRange cell = exWorkSheet.Cells[start, i];
                            cell.Value = col.ColumnName.Split('|')[0];
                            exWorkSheet.Row(start).Hidden = true;
                            exWorkSheet.Row(start).Style.Font.Color.SetColor(System.Drawing.Color.Transparent);
                        }
                        start++;
                        for (int i = 1; i <= len; i++)
                        {
                            var col = table.Columns[i - 1];
                            ExcelRange cell = exWorkSheet.Cells[start, i];
                            cell.Value = col.ColumnName.Split('|')[1];
                            cell.Style.Indent = 1;
                            cell.Style.Font.Bold = true;
                            exWorkSheet.Column(i).Style.Font.Size = 12;
                            exWorkSheet.Column(i).Style.WrapText = true;
                            exWorkSheet.Column(i).Width = int.Parse(col.ColumnName.Split('|')[2]);
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3] == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Left;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        }
                        exWorkSheet.Cells[start, 1, start, len].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        exWorkSheet.Cells[start, 1, start, len].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#0078d4"));
                        exWorkSheet.Cells[start, 1, start, len].Style.Font.Color.SetColor(System.Drawing.Color.White);
                        #endregion
                        topcell1.Value = string.Format("DANH SÁCH NGƯỜI DÙNG");
                        for (int k = 0; k < table.Rows.Count; k++)
                        {
                            start += 1;
                            var dr = table.Rows[k];
                            for (int i = 1; i <= len; i++)
                            {
                                var col = table.Columns[i - 1];
                                ExcelRange cell = exWorkSheet.Cells[start, i];
                                try
                                {
                                    cell.Value = dr[col.ColumnName].ToString();
                                    cell.Style.Indent = 1;
                                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3] == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Left;
                                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                }
                                catch (Exception e) { }
                            }
                        }
                        #endregion
                        Byte[] fileBytes = exportPackge.GetAsByteArray();
                        string fname = helper.convertToUnSign3(proc.excelname + uid);
                        string path = HttpContext.Current.Server.MapPath("~/Portals/Export/" + fname + ".xlsx");
                        File.WriteAllBytes(path, fileBytes);
                        return Request.CreateResponse(HttpStatusCode.OK, new { path = "/Portals/Export/" + fname + ".xlsx", err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
        public HttpResponseMessage ExportHTML([System.Web.Mvc.Bind(Include = "lib,name,html,opition")][FromBody] modelHTML model)
        {
            var identity = User.Identity as ClaimsIdentity;
            IEnumerable<Claim> claims = identity.Claims;
            string ip = getipaddress();
            string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
            string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
            string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
            bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
            string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
            List<string> delfiles = new List<string>();
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
                using (var htmlStream = new MemoryStream(Encoding.UTF8.GetBytes(model.html)))
                {
                    string path = Path.Combine(HttpContext.Current.Server.MapPath("~/Portals/Export/") + model.name);
                    if (model.lib == "Gembox")
                    {
                        #region Gembox
                        ComponentInfo.SetLicense("DTZX-HTZ5-B7Q6-2GA6");
                        var htmlLoadOptions = new HtmlLoadOptions();
                        var document = DocumentModel.Load(htmlStream, htmlLoadOptions);
                        GemBox.Document.Section section = document.Sections[0];
                        GemBox.Document.PageSetup pageSetup = section.PageSetup;
                        PageMargins pageMargins = pageSetup.PageMargins;
                        pageMargins.Left = 55.5;
                        pageMargins.Top = pageMargins.Bottom = pageMargins.Right = 45.4;
                        SaveOptions opit = SaveOptions.DocxDefault;
                        document.Save(path);
                        #endregion
                    }
                    else if (model.lib == "Spire")
                    {
                        #region Spire
                        Spire.License.LicenseProvider.SetLicenseKey("CtOzJs2BlzPokWgBAKMfmNxjRwLa3eqzrAvKtn54UDB/dWjIyGokcs+UQuYuvMY03wX56Ox75KV+U1r5H0PR++c1zc6i8e0QIOVuhMp9Qbg5A9bJJA7e7KvC4KMINTr4jnJy/yTGFwT1aEusw144kml/6oAttwEUoXBkDPLWGOsvNgH1iTYkTGWMXEV8Or4p4t4doNsl0Z7V5qWDKwB6sD/ZiH7l/Jum27FWevOlKIa2VG1rEKjtURYukbWXeSH54IKtmn7nmr0wKwnRgdu3q60aC/PdkxC0zX75EnbU5M6fa3pplU40f3LGOWcgZ2f+8oI7qpPXJ8/s7LrsxBqpQ2YGKfKuqx5ex9ALrXgjnwjcslmXPYun7flHGIkbvBsCjCpo4Ed+M658sZTGATak6gLmftEqhJ1ZZJJKFgXE5qa/TyCY7wIq1ll+z1VNhnSBZUc1RA4TwSBcFKvrZEHlj9o1WFZ1+QqNAcnzh/n+tG48B0wHLCl6D4hroCfWMoaw/23DRxx1WuWqfkazuz2H8ga1RC2XPs83nB7CHPFNs0sT5lsKbfA3P9jgtza5CEhfjAN/3TiwEP/tvnTZY+VABK97veB77h4LEiVMfQXzKfhm9cNW4ft/ofVU2OfqZ8GjtntoZdPxp1bIwTvI98SnQi/H81w19aHwUqNECTeJBjqqHMxdVKVSBAKJL0TM7RyzoOPKS19OfURAxlEgRUqJF/BM8eU0R+UicIM2h36sTuBKO4g3H6woDMlnx0QG0nqthauTB7oK6QFTwk44UQ1kTAu8LeOJwM2xNu5MLsPmoWwDvmIaTuZIW6VUX8C285c9KkrYAf79YKA3e3yxx6SSQdN/jLbtR7MaeGpxRzX0iEbqL9sG1m5USuYVByvVKQ4ntvfCMlLmUN9UCvJ/m63K27Z2dm6fTXIe/g0smYmnvEQ3JQVnldWOi1TKOMK8RbuU5un5mQZ96pLq0Q7g0NLQZh50UMT+OjAzXHPxmXfV6/deHeE8Gbb3ZYJSg7UXW2sty86uXwkj89x5yJTaMNtm6Kh2QQugn/Vd9n8C8QReNewYxjF827FBpMp9yf+vLf2FSyA50wiA9o9luoXYgRmGuUh+g9+KMWgMK5fxQ2h3cHqADzPcwsDhVfG6HuAgt81vH/M5hFLdQztXdvRKVuYOyyTOnQz9K93LZ2EvbeWz0YByRkGxnve+K8UNo3pyNgaPGRQWr5RbeURNJ4PhmM3dB2oMkwE//+s39ccgADdEJS8s35cjRrVEGs8JicRu6mDNqJfdHUNfLmiySMjG/ePwhYkiB2WhJ9AqpY9N7eQ3TBsAMkr34olS6eSNpaE1BjgJsljB27GDnmMAXNZeifyIYpBcqu6H9SLN5pGBF9WHcPVivjdNpMUrKQ==");
                        Spire.License.LicenseProvider.LoadLicense();
                        Document documentSpire = new Document();
                        documentSpire.LoadFromStream(htmlStream, FileFormat.Html, XHTMLValidationType.None);
                        documentSpire.SaveToFile(path);
                        #endregion
                    }
                    else if (model.lib == "OpenHtmlToPdf")
                    {
                        #region Spire
                        var opt = model.opition;
                        if (opt == null || (opt.left == 0 && opt.top == 0 && opt.right == 0 && opt.bottom == 0))
                        {
                            opt = new PDFOpition()
                            {
                                orientation = opt.orientation ?? "Portrait",
                                pageSize = opt.pageSize ?? "A4",
                                left = 2.2,
                                top = 1.8,
                                right = 1.8,
                                bottom = 1.8
                            };
                        }
                        var pdf = Pdf
                        .From(model.html.Replace("table-layout: fixed;", ""))
                        .WithTitle(helper.convertToUnSign3(model.name))
                        .WithoutOutline()
                        .WithGlobalSetting("orientation", opt.orientation)
                        .WithGlobalSetting("size.pageSize", opt.pageSize)
                        .WithGlobalSetting("web.background", "true")
                        .WithObjectSetting("web.defaultEncoding", "utf-8")
                        .WithGlobalSetting("margin.bottom", opt.bottom.Centimeters().SettingString)
                        .WithGlobalSetting("margin.left", opt.left.Centimeters().SettingString)
                        .WithGlobalSetting("margin.right", opt.right.Centimeters().SettingString)
                        .WithGlobalSetting("margin.top", opt.top.Centimeters().SettingString)
                        .Portrait()
                        .Comressed()
                        .Content();

                        string pathWrite = HttpContext.Current.Server.MapPath("~/Portals/Export/") + Path.GetFileName(model.name);
                        File.WriteAllBytes(pathWrite, pdf);
                        #endregion
                    }
                    else
                    {
                        var documentConverter = new DocumentConverter(htmlStream, DocumentFormat.Html);
                        documentConverter.ConvertTo(HttpContext.Current.Server.MapPath("~/Portals/Export/") + model.name);
                    }
                }
            }
            catch (Exception e)
            {
                string contents = helper.ExceptionMessage(e);
                helper.saveLog(uid, name, JsonConvert.SerializeObject(new
                {
                    contents
                }), domainurl + "Excel/ExportHTML", ip, tid, "Lỗi khi kết xuất", 0, "Excel");
                if (!helper.debug)
                {
                    contents = "";
                }
                Log.Error(contents);
                return Request.CreateResponse(HttpStatusCode.OK, new { ms = contents, err = "1" });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { path = "/Portals/Export/" + model.name, err = "0" });
        }
        [HttpPost]
        public HttpResponseMessage ExportExcelDynamic([System.Web.Mvc.Bind(Include = "excelname,header,title,rows")][FromBody] modelExcel model)
        {
            using (DBEntities db = new DBEntities())
            {
                var identity = User.Identity as ClaimsIdentity;
                IEnumerable<Claim> claims = identity.Claims;
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
                string ip = getipaddress();
                string name = claims.Where(p => p.Type == "fname").FirstOrDefault()?.Value;
                string tid = claims.Where(p => p.Type == "tid").FirstOrDefault()?.Value;
                string uid = claims.Where(p => p.Type == "uid").FirstOrDefault()?.Value;
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";
                try
                {
                    string Connection = db.Database.Connection.ConnectionString;
                    using (ExcelPackage exportPackge = new ExcelPackage())
                    {
                        String FileName = String.Empty;
                        FileName = model.excelname + "_" + DateTime.Now.ToString("ddMMyyyy");
                        ExcelWorksheet exWorkSheet = exportPackge.Workbook.Worksheets.Add(FileName);
                        #region thead
                        var len = model.header.Count;
                        ExcelRange titlecell = exWorkSheet.Cells[1, 1, 1, len];
                        titlecell.Style.Font.Bold = true;
                        titlecell.Style.Font.Size = 16;
                        titlecell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        titlecell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        titlecell.Merge = true;
                        titlecell.Value = model.title;
                        titlecell.Style.WrapText = true;
                        exWorkSheet.Row(1).Height = 40;
                        //Từ dòng, cột đến dòng cột
                        exWorkSheet.Row(2).Height = model.header[0].Height ?? 20;
                        for (int i = 1; i <= model.header.Count; i++)
                        {
                            var header = model.header[i - 1];
                            var cell = exWorkSheet.Cells[2, i];
                            cell.Style.Font.Bold = header.Bold;
                            cell.Style.Font.Size = header.FontSize ?? 16;
                            cell.Style.HorizontalAlignment = header.Align == "center" ? ExcelHorizontalAlignment.Center : header.Align == "left" ? ExcelHorizontalAlignment.Left : ExcelHorizontalAlignment.Right;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            cell.Style.WrapText = true;
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(header.BgColor));
                            cell.Style.Font.Color.SetColor(ColorTranslator.FromHtml(header.Color));
                            cell.Value = header.title;
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            exWorkSheet.Column(i).Width = header.Width ?? 20;

                        }
                        for (int k = 0; k < model.rows.Count; k++)
                        {
                            int start = k + 3;
                            var row = model.rows[k];
                            for (int i = 1; i <= row.Count; i++)
                            {
                                var header = row[i - 1];
                                if (header.title == null) continue;
                                var cell = exWorkSheet.Cells[start, i];
                                cell.Style.Font.Bold = header.Bold;
                                cell.Style.Font.Size = header.FontSize ?? 16;
                                cell.Style.HorizontalAlignment = header.Align == "center" ? ExcelHorizontalAlignment.Center : header.Align == "left" ? ExcelHorizontalAlignment.Left : ExcelHorizontalAlignment.Right;
                                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                cell.Style.WrapText = true;
                                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(header.BgColor));
                                cell.Style.Font.Color.SetColor(ColorTranslator.FromHtml(header.Color));
                                cell.Value = header.title;
                                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                if (header.Rowspan > 1)
                                {
                                    exWorkSheet.Cells[start, i, start + header.Rowspan - 1, i].Merge = true;
                                }
                                if (header.Colspan > 1)
                                {
                                    exWorkSheet.Cells[start, i, start, header.Colspan - 1 + i].Merge = true;
                                }
                                if (header.Width != null)
                                    exWorkSheet.Column(i).Width = header.Width ?? 20;
                                if (header.Height != null)
                                    exWorkSheet.Row(start).Height = header.Height ?? 20;
                            }

                        }
                        #endregion
                        titlecell.Value = model.title;
                        Byte[] fileBytes = exportPackge.GetAsByteArray();
                        string fname = helper.convertToUnSign3(model.excelname + "_" + uid);
                        string path = HttpContext.Current.Server.MapPath("~/Portals/Export/" + fname + ".xlsx");
                        File.WriteAllBytes(path, fileBytes);
                        return Request.CreateResponse(HttpStatusCode.OK, new { path = "/Portals/Export/" + fname + ".xlsx", err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "Excel/ExportExcelDynamic", ip, tid, "Lỗi khi kết xuất file (model.excelname)", 0, "Excel");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = model, contents }), domainurl + "Excel/ExportExcelDynamic", ip, tid, "Lỗi khi kết xuất file (model.excelname)", 0, "Excel");
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
        public async Task<HttpResponseMessage> ExportExcelWithLogo([System.Web.Mvc.Bind(Include = "excelname,par,proc")][FromBody] sqlProc proc)
        {
            using (DBEntities db = new DBEntities())
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
                int dvid = int.Parse(claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value);
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string root = HttpContext.Current.Server.MapPath("~/Portals");

                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

                try
                {
                    string Connection = db.Database.Connection.ConnectionString;
                    using (ExcelPackage exportPackge = new ExcelPackage())
                    {
                        String FileName = String.Empty;
                        FileName = proc.excelname + "_" + DateTime.Now.ToString("ddMMyyyy");
                        ExcelWorksheet exWorkSheet = exportPackge.Workbook.Worksheets.Add(FileName);
                        #region tbody
                        var sqlpas = new List<SqlParameter>();
                        if (proc != null && proc.par != null)
                        {
                            foreach (sqlPar p in proc.par)
                            {
                                sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                            }
                        }
                        var arrpas = sqlpas.ToArray();
                        var task = Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables[0]);
                        var table = await task;
                        #region thead
                        var org = db.sys_organization.AsNoTracking().Where(x => x.organization_id == dvid).FirstOrDefault();

                        var len = table.Columns.Count;
                        int start = 3;
                        ExcelRange topcell1 = exWorkSheet.Cells[1, 1, 1, len];


                        Image img = Image.FromFile(root + org.logo.Substring(8));
                        ExcelPicture art = exWorkSheet.Drawings.AddPicture("Logo", img);
                        art.SetSize(100, 56);

                        art.SetPosition(0, 10, 1, 1);



                        exWorkSheet.Row(1).Height = 56;
                        exWorkSheet.Row(2).Height = 20;
                        exWorkSheet.Row(3).Height = 25;
                        exWorkSheet.Row(4).Height = 25;
                        topcell1.Style.Font.Bold = true;
                        topcell1.Style.Font.Size = 16;
                        topcell1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        topcell1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        topcell1.Merge = true;
                        topcell1.Style.WrapText = true;
                        ExcelRange topcell2 = exWorkSheet.Cells[2, 1, 2, len];
                        topcell2.Value = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
                        topcell2.Style.Font.Italic = true;
                        topcell2.Style.Font.Bold = false;
                        topcell2.Style.Font.Size = 12;
                        topcell2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        topcell2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        topcell2.Merge = true;
                        topcell2.Style.WrapText = true;
                        for (int i = 1; i <= len; i++)
                        {
                            var col = table.Columns[i - 1];
                            ExcelRange cell = exWorkSheet.Cells[start, i];
                            cell.Value = col.ColumnName.Split('|')[0];
                            exWorkSheet.Row(start).Hidden = true;
                            exWorkSheet.Row(start).Style.Font.Color.SetColor(System.Drawing.Color.Transparent);
                        }
                        start++;
                        for (int i = 1; i <= len; i++)
                        {
                            var col = table.Columns[i - 1];
                            ExcelRange cell = exWorkSheet.Cells[start, i];
                            cell.Value = col.ColumnName.Split('|')[1];
                            cell.Style.Indent = 1;
                            cell.Style.Font.Bold = true;
                            exWorkSheet.Column(i).Style.Font.Size = 12;
                            exWorkSheet.Column(i).Style.WrapText = true;
                            exWorkSheet.Column(i).Width = int.Parse(col.ColumnName.Split('|')[2]);
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3].ToUpper() == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        }
                        exWorkSheet.Cells[start, 1, start, len].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        exWorkSheet.Cells[start, 1, start, len].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#0078d4"));
                        exWorkSheet.Cells[start, 1, start, len].Style.Font.Color.SetColor(System.Drawing.Color.White);
                        #endregion
                        topcell1.Value = string.Format(proc.excelname + " ({0})", table.Rows.Count);
                        for (int k = 0; k < table.Rows.Count; k++)
                        {
                            start += 1;
                            var dr = table.Rows[k];
                            for (int i = 1; i <= len; i++)
                            {
                                var col = table.Columns[i - 1];
                                ExcelRange cell = exWorkSheet.Cells[start, i];
                                ExcelRange cellMauNen = exWorkSheet.Cells[4, 4];
                                ExcelRange cellMauChu = exWorkSheet.Cells[4, 5];
                                try
                                {
                                    if (dr[col.ColumnName].ToString().Contains("Portal") == true && proc.excelname == "DANH SÁCH TEM")
                                    {
                                        exWorkSheet.Row(i + 2).Height = 150;
                                        // cell.Value = dr[col.ColumnName].ToString() + "1";
                                        string strPath11 = HttpContext.Current.Server.MapPath(dr[col.ColumnName].ToString());
                                        System.Web.UI.WebControls.Image TEST_IMAGE = new System.Web.UI.WebControls.Image();

                                        var url = dr[col.ColumnName] != null ? dr[col.ColumnName].ToString() : "";
                                        url = Regex.Replace(url.Replace("\\", "/"), @"\.*/+", "/");
                                        var listPath = url.Split('/');
                                        var pathFile = "";
                                        foreach (var item in listPath)
                                        {
                                            if (item.Trim() != "")
                                            {
                                                pathFile += "/" + Path.GetFileName(item);
                                            }
                                        }
                                        if (!File.Exists(HttpContext.Current.Server.MapPath("~/") + pathFile))
                                        //string pathFileInExport = HttpContext.Current.Server.MapPath("~/Portals");
                                        //if (dr[col.ColumnName].ToString().Contains("Emote"))
                                        //{
                                        //    pathFileInExport += "/Emote";
                                        //}
                                        //if (!File.Exists(Path.Combine(pathFileInExport, Path.GetFileName(strPath11))))
                                        {
                                            cell.Value = dr[col.ColumnName].ToString() + " (File đã bị xóa hoặc chuyển đi nơi khác!)";
                                        }
                                        else
                                        {
                                            System.Drawing.Image myImage = System.Drawing.Image.FromFile(strPath11);
                                            var pic = exWorkSheet.Drawings.AddPicture(dr[col.ColumnName].ToString(), myImage);
                                            pic.SetSize(180, 180);
                                            // Row, RowoffsetPixel, Column, ColumnOffSetPixel
                                            pic.SetPosition(i + 1, 10, 2, 12);
                                        }
                                    }
                                    else
                                    {
                                        cell.Value = dr[col.ColumnName].ToString();
                                    }
                                    cell.Style.Indent = 1;
                                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3].ToUpper() == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Left;
                                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                }
                                catch (Exception e) { }
                            }
                        }
                        #endregion
                        Byte[] fileBytes = exportPackge.GetAsByteArray();
                        string fname = helper.convertToUnSign3(proc.excelname.ToLower() + "_" + uid);
                        var strPath = HttpContext.Current.Server.MapPath("~/Portals/Export");
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);
                        string path = HttpContext.Current.Server.MapPath("~/Portals/Export/" + fname + ".xlsx");
                        File.WriteAllBytes(path, fileBytes);
                        return Request.CreateResponse(HttpStatusCode.OK, new { path = "/Portals/Export/" + fname + ".xlsx", err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
        public async Task<HttpResponseMessage> ExportTreeExcelWithLogo([System.Web.Mvc.Bind(Include = "excelname,par,proc")][FromBody] sqlProc proc)
        {
            using (DBEntities db = new DBEntities())
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
                int dvid = int.Parse(claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value);
                bool ad = claims.Where(p => p.Type == "ad").FirstOrDefault()?.Value == "True";
                string root = HttpContext.Current.Server.MapPath("~/Portals");

                string domainurl = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port + "/";

                try
                {
                    string Connection = db.Database.Connection.ConnectionString;
                    using (ExcelPackage exportPackge = new ExcelPackage())
                    {
                        String FileName = String.Empty;
                        FileName = proc.excelname + "_" + DateTime.Now.ToString("ddMMyyyy");
                        ExcelWorksheet exWorkSheet = exportPackge.Workbook.Worksheets.Add(FileName);
                        #region tbody
                        var sqlpas = new List<SqlParameter>();
                        if (proc != null && proc.par != null)
                        {
                            foreach (sqlPar p in proc.par)
                            {
                                sqlpas.Add(new SqlParameter("@" + p.par, p.va));
                            }
                        }
                        var arrpas = sqlpas.ToArray();
                        var taskParent = Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables[0]);
                        var taskChild = Task.Run(() => SqlHelper.ExecuteDataset(Connection, proc.proc, arrpas).Tables[1]);
                        var table = await taskParent;
                        var tableChild = await taskChild;

                        #region thead
                        var org = db.sys_organization.AsNoTracking().Where(x => x.organization_id == dvid).FirstOrDefault();

                        var len = table.Columns.Count - 2;


                        int start = 3;
                        ExcelRange topcell1 = exWorkSheet.Cells[1, 1, 1, len];


                        Image img = Image.FromFile(root + org.logo.Substring(8));
                        ExcelPicture art = exWorkSheet.Drawings.AddPicture("Logo", img);
                        art.SetSize(100, 56);

                        art.SetPosition(0, 10, 1, 1);



                        exWorkSheet.Row(1).Height = 56;
                        exWorkSheet.Row(2).Height = 20;
                        exWorkSheet.Row(3).Height = 25;
                        exWorkSheet.Row(4).Height = 25;
                        topcell1.Style.Font.Bold = true;
                        topcell1.Style.Font.Size = 16;
                        topcell1.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        topcell1.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        topcell1.Merge = true;
                        topcell1.Style.WrapText = true;
                        ExcelRange topcell2 = exWorkSheet.Cells[2, 1, 2, len];
                        topcell2.Value = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
                        topcell2.Style.Font.Italic = true;
                        topcell2.Style.Font.Bold = false;
                        topcell2.Style.Font.Size = 12;
                        topcell2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        topcell2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        topcell2.Merge = true;
                        topcell2.Style.WrapText = true;
                        ExcelRange cell1 = exWorkSheet.Cells[start, 1];
                        cell1.Value = "STT";
                        for (int i = 2; i <= len; i++)
                        {
                            var col = table.Columns[i - 1];
                            ExcelRange cell = exWorkSheet.Cells[start, i];
                            cell.Value = col.ColumnName.Split('|')[0];
                            exWorkSheet.Row(start).Hidden = true;
                            exWorkSheet.Row(start).Style.Font.Color.SetColor(System.Drawing.Color.Transparent);
                        }
                        start++;
                        ExcelRange cell11 = exWorkSheet.Cells[start, 1];
                        cell11.Value = "STT";
                        cell11.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        cell11.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        cell11.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        cell11.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        cell11.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        cell11.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        cell11.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        for (int i = 2; i <= len; i++)
                        {
                            
                            var col = table.Columns[i - 1];
                            ExcelRange cell = exWorkSheet.Cells[start, i];
                            cell.Value = col.ColumnName.Split('|')[1];
                            cell.Style.Indent = 1;
                            cell.Style.Font.Bold = true;
                            exWorkSheet.Column(i).Style.Font.Size = 12;
                            exWorkSheet.Column(i).Style.WrapText = true;
                            exWorkSheet.Column(i).Width = int.Parse(col.ColumnName.Split('|')[2]);
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3].ToUpper() == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Center;
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        }
                        exWorkSheet.Cells[start, 1, start, len].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        exWorkSheet.Cells[start, 1, start, len].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#0078d4"));
                        exWorkSheet.Cells[start, 1, start, len].Style.Font.Color.SetColor(System.Drawing.Color.White);
                        #endregion
                        var sttS = 0;
                        topcell1.Value = string.Format(proc.excelname + " ({0})", table.Rows.Count);
                        for (int k = 0; k < table.Rows.Count; k++)
                        {
                            start += 1;
                           
                            var dr = table.Rows[k];
                            sttS += 1;
                            ExcelRange cell12 = exWorkSheet.Cells[start, 1];
                            cell12.Value = sttS;
                         
                            
                            cell12.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell12.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            for (int i = 2; i <= len; i++)
                            {
                                var col = table.Columns[i - 1];
                                ExcelRange cell = exWorkSheet.Cells[start, i];
                                try
                                {
                                    cell.Value = dr[col.ColumnName].ToString();
                                    cell.Style.Indent = 1;
                                    cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                    cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3].ToUpper() == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Left;
                                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                }
                                catch (Exception e) { }
                            }
                            var syt = tableChild.Rows.Count;


                            for (int l = 0; l < syt; l++)
                            {

                                var drx = tableChild.Rows[l];
                                var colP = table.Columns[syt - 1];
                                var colC = tableChild.Columns[syt - 2];
                                if (drx[colC.ColumnName].ToString() == dr[colP.ColumnName].ToString())
                                {
                                    start += 1;
                                    sttS += 1;
                                    ExcelRange cell13 = exWorkSheet.Cells[start, 1];
                                    cell13.Value = sttS;
                          
                                    cell13.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    cell13.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    for (int i = 2; i <= len; i++)
                                    {
                                        var col = table.Columns[i - 1];
                                        ExcelRange cell = exWorkSheet.Cells[start, i];
                                        try
                                        {
                                            cell.Value = drx[i - 1].ToString();
                                            cell.Style.Indent = 1;
                                            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                            cell.Style.HorizontalAlignment = col.ColumnName.Split('|')[3].ToUpper() == "C" ? ExcelHorizontalAlignment.Center : ExcelHorizontalAlignment.Left;
                                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                        }
                                        catch (Exception e) { }
                                    }
                                    //tableChild.Rows[l].Delete();
                                    //syt= tableChild.Rows.Count;
                                }
                            }
                        }
                        #endregion
                        Byte[] fileBytes = exportPackge.GetAsByteArray();
                        string fname = helper.convertToUnSign3(proc.excelname.ToLower() + "_" + uid);
                        var strPath = HttpContext.Current.Server.MapPath("~/Portals/Export");
                        bool exists = Directory.Exists(strPath);
                        if (!exists)
                            Directory.CreateDirectory(strPath);
                        string path = HttpContext.Current.Server.MapPath("~/Portals/Export/" + fname + ".xlsx");
                        File.WriteAllBytes(path, fileBytes);
                        return Request.CreateResponse(HttpStatusCode.OK, new { path = "/Portals/Export/" + fname + ".xlsx", err = "0" });
                    }
                }
                catch (DbEntityValidationException e)
                {
                    string contents = helper.getCatchError(e, null);
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
                    helper.saveLog(uid, name, JsonConvert.SerializeObject(new { data = proc, contents }), domainurl + "Excel/ExportExcel", ip, tid, "Lỗi khi kết xuất file (proc.excelname)", 0, "Excel");
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
}