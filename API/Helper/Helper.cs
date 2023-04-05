using API.Models;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using System.Security.Cryptography.Xml;
using SocketIOClient;
using System.Net.Sockets;
using API.Helper;
using System.Net.Http;
using System.Configuration;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;

namespace Helper
{
    public static class helper
    {
        public static SocketIO socketClient;
        public static string socketUrl = "wss://socket2.soe.vn/";
        public static string publicprockey = "vYIrl2C30cdvhjyroM0HYQr2fs7cNo9Qx01g8P1nPIS7joDA";
        public static string tokenkey = "101219881502198921112013";
        //public static string passkey = "1012198815021989";
        public static string psKey = "1012198815021989";
        public static string keyConnect = "1502198910121988";
        public static bool isEncodeProc = false;
        public static bool socket = true;
        public static bool debug = true;
        public static string logCongtent = "Có lỗi xảy ra, vui lòng thử lại!";
        public static bool wlog = true;
        public static int milisec = 1000;
        public static int timemail = 5;// phut
        public static int timeout = 60 * 24 * 30;//phút
        public static string publictoken = null;//phút
        public const int ImageMinimumBytes = 512;
        public static string path_xml = null;
        public static string getDecideName(string userAgent)
        {
            Regex OS = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Regex device = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string device_info = string.Empty;
            if (OS.IsMatch(userAgent))
            {
                device_info = OS.Match(userAgent).Groups[0].Value;
            }
            if (device.IsMatch(userAgent.Substring(0, 4)))
            {
                device_info += device.Match(userAgent).Groups[0].Value;
            }
            if (!string.IsNullOrEmpty(device_info))
            {
                return "Mobile";
            }
            return "PC";

        }

        public static object CheckUpdateObject(object originalObj, object updateObj)
        {
            foreach (var property in updateObj.GetType().GetProperties())
            {
                if (property.GetValue(updateObj, null) == null)
                {
                    property.SetValue(updateObj, originalObj.GetType().GetProperty(property.Name)
                    .GetValue(originalObj, null));
                }
            }
            return updateObj;
        }
        public static Object ConvertValue(string typeInString, string value)
        {
            Type originalType = Type.GetType(typeInString);

            var underlyingType = Nullable.GetUnderlyingType(originalType);

            // if underlyingType has null value, it means the original type wasn't nullable
            object instance = Convert.ChangeType(value, underlyingType ?? originalType);

            return instance;
        }
        public static bool is_admin(string rid)
        {
            return rid == "administrator" || rid == "admin";
        }
        public static List<InfoColumn> bindInfoColumn(DataTable table)
        {
            List<InfoColumn> ifocolumn = new List<InfoColumn>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var dr = table.Rows[i];
                double? maxl = null;
                if (dr["character_maximum_length"] != null && dr["character_maximum_length"].ToString() != "NULL")
                {
                    try
                    {
                        maxl = double.Parse(dr["character_maximum_length"].ToString());
                    }
                    catch
                    {

                    }
                }
                ifocolumn.Add(new InfoColumn
                {
                    Column = dr["Column"].ToString(),
                    Description = dr["Description"] == null ? null : dr["Description"].ToString(),
                    data_type = dr["data_type"].ToString(),
                    character_maximum_length = maxl,
                    is_nullable = dr["data_type"].ToString() == "YES"
                });
            }
            return ifocolumn;
        }
        public static string GenKey()
        {
            return System.Guid.NewGuid().ToString("N").ToUpper();
        }
        public static void SaveByteArrayAsImage(string fullOutputPath, string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);

            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }

            image.Save(fullOutputPath, System.Drawing.Imaging.ImageFormat.Png);
        }
        public static bool IsBase64String(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }
        public static bool IsVideoFileName(string name)
        {
            return name.ToLower().Contains(".avi")
              || name.ToLower().Contains(".mp4")
              || name.ToLower().Contains(".wmv")
              || name.ToLower().Contains(".mkv");

        }
        public static bool IsAudioFileName(string name)
        {
            return name.ToLower().Contains(".mp3")
              || name.ToLower().Contains(".wma")
              || name.ToLower().Contains(".wav")
              || name.ToLower().Contains(".flac")
              || name.ToLower().Contains(".aac")
              || name.ToLower().Contains(".ogg");

        }
        #region Name To Tag  
        public static string UniqueFileName(string name)
        {
            try
            {
                int i = name.LastIndexOf(".");
                return name.Substring(0, i - 1) + DateTime.Now.ToString("yyMMddHHmmssff") + name.Substring(i);
            }
            catch
            {
                return name;
            }
        }

        public static string convertToUnSign(string s)
        {
            string stFormD = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            return (sb.ToString().Normalize(NormalizationForm.FormD));
        }

        public static string convertToUnSignNoSpace(string s)
        {
            string stFormD = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            sb = sb.Replace(" ", "");
            return (sb.ToString().Normalize(NormalizationForm.FormD));
        }

        #endregion

        public static List<Dictionary<string, object>> GetTableRows(DataTable dtData)
        {
            List<Dictionary<string, object>>
            lstRows = new List<Dictionary<string, object>>();
            Dictionary<string, object> dictRow = null;

            foreach (DataRow dr in dtData.Rows)
            {
                dictRow = new Dictionary<string, object>();
                foreach (DataColumn col in dtData.Columns)
                {
                    dictRow.Add(col.ColumnName, dr[col]);
                }
                lstRows.Add(dictRow);
            }
            return lstRows;
        }
        public static string getDecideNameAuto(string ua)
        {
            string[] MobileDevices = new string[] { "iPhone", "iPad","iPod","BlackBerry",
                                                     "Nokia", "Android", "WindowsPhone","SamSung","LG","Sony",
                                                     "Mobile"
                                                     };
            foreach (string MobileDeviceName in MobileDevices)
            {
                if ((ua.IndexOf(MobileDeviceName, StringComparison.OrdinalIgnoreCase)) > 0)
                {
                    return MobileDeviceName;

                }
            }
            return "PC";

        }
        public static string Decrypt(string strKey, string strData)
        {
            if (String.IsNullOrEmpty(strData))
            {
                return "";
            }
            string strValue = "";
            if (!String.IsNullOrEmpty(strKey))
            {
                //convert key to 16 characters for simplicity
                if (strKey.Length < 16)
                {
                    strKey = strKey + "XXXXXXXXXXXXXXX".Substring(0, 16 - strKey.Length);
                }
                else
                {
                    strKey = strKey.Substring(0, 16);
                }

                //create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(strKey.Substring(0, 8));
                byte[] byteVector = Encoding.UTF8.GetBytes(strKey.Substring(strKey.Length - 8, 8));

                //convert data to byte array and Base64 decode
                var byteData = new byte[strData.Length];
                try
                {
                    byteData = Convert.FromBase64String(strData);
                }
                catch //invalid length
                {
                    strValue = strData;
                }
                if (String.IsNullOrEmpty(strValue))
                {
                    try
                    {
                        //decrypt
                        var objDES = new DESCryptoServiceProvider();
                        var objMemoryStream = new MemoryStream();
                        var objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateDecryptor(byteKey, byteVector), CryptoStreamMode.Write);
                        objCryptoStream.Write(byteData, 0, byteData.Length);
                        objCryptoStream.FlushFinalBlock();

                        //convert to string
                        Encoding objEncoding = Encoding.UTF8;
                        strValue = objEncoding.GetString(objMemoryStream.ToArray());
                    }
                    catch //decryption error
                    {
                        strValue = "";
                    }
                }
            }
            else
            {
                strValue = strData;
            }
            return strValue;
        }
        public static string Encrypt(string strKey, string strData)
        {
            if (string.IsNullOrWhiteSpace(strData))
            {
                return "";
            }
            string strValue = "";
            if (!String.IsNullOrEmpty(strKey))
            {
                //convert key to 16 characters for simplicity
                if (strKey.Length < 16)
                {
                    strKey = strKey + "XXXXXXXXXXXXXXX".Substring(0, 16 - strKey.Length);
                }
                else
                {
                    strKey = strKey.Substring(0, 16);
                }

                //create encryption keys
                byte[] byteKey = Encoding.UTF8.GetBytes(strKey.Substring(0, 8));
                byte[] byteVector = Encoding.UTF8.GetBytes(strKey.Substring(strKey.Length - 8, 8));

                //convert data to byte array
                byte[] byteData = Encoding.UTF8.GetBytes(strData);

                //encrypt 
                var objDES = new DESCryptoServiceProvider();
                var objMemoryStream = new MemoryStream();
                var objCryptoStream = new CryptoStream(objMemoryStream, objDES.CreateEncryptor(byteKey, byteVector), CryptoStreamMode.Write);
                objCryptoStream.Write(byteData, 0, byteData.Length);
                objCryptoStream.FlushFinalBlock();

                //convert to string and Base64 encode
                strValue = Convert.ToBase64String(objMemoryStream.ToArray());
            }
            else
            {
                strValue = strData;
            }
            return strValue;
        }

        public static string ExceptionMessage(Exception e)
        {
            string StackTrace = e.StackTrace;
            var messages = new List<string>();
            do
            {
                messages.Add(e.Message);
                e = e.InnerException;
            }
            while (e != null);
            var message = string.Join("\n", messages);
            return message + "\n" + StackTrace;
        }
        public static string getCatchError(DbEntityValidationException e, Exception ex)
        {
            string contents = "";
            try
            {
                if (e != null)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        contents += ("Entity of type \"" + eve.Entry.Entity.GetType().Name + "\" in state \"" + eve.Entry.State + "\" has the following validation errors:\n");
                        foreach (var ve in eve.ValidationErrors)
                        {
                            contents += "- Property: \"" + ve.PropertyName + "\", Error: \"" + ve.ErrorMessage + "\"\n";
                        }
                    }
                }
                else if (ex != null)
                {
                    contents = ExceptionMessage(ex);
                }
            }
            catch (Exception ec)
            {
                return ec.ToString();
            }

            return contents;
        }
        public static string encryptXML(string xmlPath, string eleName, string keyName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.Load(xmlPath);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            RSA rsaKey = RSA.Create();

            try
            {
                // Encrypt the "creditcard" element.
                XmlDocument res = encryptXML_RSA(xmlDoc, eleName, rsaKey, keyName);
                res.Save(xmlPath);
                return "OK";
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                // Clear the RSA key.
                rsaKey.Clear();
            }
        }
        public static string decryptXML(string xmlPath, string keyName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.PreserveWhitespace = true;
                xmlDoc.Load(xmlPath);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            RSA rsaKey = RSA.Create();

            try
            {
                // Decrypt the "creditcard" element.
                string res = decryptXML_RSA(xmlDoc, rsaKey, keyName);
                return res;
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                // Clear the RSA key.
                rsaKey.Clear();
            }
        }
        private static XmlDocument encryptXML_RSA(XmlDocument Doc, string ElementToEncrypt, RSA Alg, string KeyName)
        {
            // Check the arguments.
            if (Doc == null)
                throw new ArgumentNullException("Doc");
            if (ElementToEncrypt == null)
                throw new ArgumentNullException("ElementToEncrypt");
            if (Alg == null)
                throw new ArgumentNullException("Alg");

            ////////////////////////////////////////////////
            // Find the specified element in the XmlDocument
            // object and create a new XmlElemnt object.
            ////////////////////////////////////////////////

            XmlElement elementToEncrypt = Doc.GetElementsByTagName(ElementToEncrypt)[0] as XmlElement;

            // Throw an XmlException if the element was not found.
            if (elementToEncrypt == null)
            {
                throw new XmlException("The specified element was not found");
            }

            //////////////////////////////////////////////////
            // Create a new instance of the EncryptedXml class
            // and use it to encrypt the XmlElement with the
            // a new random symmetric key.
            //////////////////////////////////////////////////

            // Create a 256 bit Aes key.
            Aes sessionKey = Aes.Create();
            sessionKey.KeySize = 256;

            EncryptedXml eXml = new EncryptedXml();

            byte[] encryptedElement = eXml.EncryptData(elementToEncrypt, sessionKey, false);

            ////////////////////////////////////////////////
            // Construct an EncryptedData object and populate
            // it with the desired encryption information.
            ////////////////////////////////////////////////

            EncryptedData edElement = new EncryptedData();
            edElement.Type = EncryptedXml.XmlEncElementUrl;

            // Create an EncryptionMethod element so that the
            // receiver knows which algorithm to use for decryption.

            edElement.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncAES256Url);

            // Encrypt the session key and add it to an EncryptedKey element.
            EncryptedKey ek = new EncryptedKey();

            byte[] encryptedKey = EncryptedXml.EncryptKey(sessionKey.Key, Alg, false);

            ek.CipherData = new CipherData(encryptedKey);

            ek.EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSA15Url);

            // Set the KeyInfo element to specify the
            // name of the RSA key.

            // Create a new KeyInfo element.
            edElement.KeyInfo = new KeyInfo();

            // Create a new KeyInfoName element.
            KeyInfoName kin = new KeyInfoName();

            // Specify a name for the key.
            kin.Value = KeyName;

            // Add the KeyInfoName element to the
            // EncryptedKey object.
            ek.KeyInfo.AddClause(kin);

            // Add the encrypted key to the
            // EncryptedData object.

            edElement.KeyInfo.AddClause(new KeyInfoEncryptedKey(ek));

            // Add the encrypted element data to the
            // EncryptedData object.
            edElement.CipherData.CipherValue = encryptedElement;

            ////////////////////////////////////////////////////
            // Replace the element from the original XmlDocument
            // object with the EncryptedData element.
            ////////////////////////////////////////////////////

            EncryptedXml.ReplaceElement(elementToEncrypt, edElement, false);
            return Doc;
        }

        private static string decryptXML_RSA(XmlDocument Doc, RSA Alg, string KeyName)
        {
            // Check the arguments.
            if (Doc == null)
                throw new ArgumentNullException("Doc");
            if (Alg == null)
                throw new ArgumentNullException("Alg");
            if (KeyName == null)
                throw new ArgumentNullException("KeyName");

            // Create a new EncryptedXml object.
            EncryptedXml exml = new EncryptedXml(Doc);

            // Add a key-name mapping.
            // This method can only decrypt documents
            // that present the specified key name.
            exml.AddKeyNameMapping(KeyName, Alg);

            // Decrypt the element.
            exml.DecryptDocument();
            return "OK";
        }
        #region Date
        public static DateTime FirstDayOfWeek(this DateTime dt)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var diff = dt.DayOfWeek - culture.DateTimeFormat.FirstDayOfWeek;
            if (diff < 0)
                diff += 7;
            var d = dt.AddDays(-diff).Date;
            return d;
        }
        public static DateTime getFirstDayOfWeek(DateTime fdt, DateTime dt)
        {
            if (fdt < FirstDayOfMonth(dt))
            {
                fdt = FirstDayOfMonth(dt);
            }
            return fdt;
        }
        public static DateTime LastDayOfWeek(this DateTime dt)
        {
            var d = FirstDayOfWeek(dt).AddDays(6);
            if (d > LastDayOfMonth(dt))
            {
                d = LastDayOfMonth(dt);
            }
            return d;
        }
        public static DateTime FirstDayOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }
        public static DateTime LastDayOfMonth(this DateTime dt)
        {
            return FirstDayOfMonth(dt).AddMonths(1).AddDays(-1);
        }
        public static DateTime FirstDayOfNextMonth(this DateTime dt)
        {
            return FirstDayOfMonth(dt).AddMonths(1);
        }
        public static string getDaystring(this DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "<b>Thứ hai</b><br/>" + dt.ToString("dd/MM/yyyy");
                case DayOfWeek.Tuesday:
                    return "<b>Thứ ba</b><br/>" + dt.ToString("dd/MM/yyyy");
                case DayOfWeek.Wednesday:
                    return "<b>Thứ tư</b><br/>" + dt.ToString("dd/MM/yyyy");
                case DayOfWeek.Thursday:
                    return "<b>Thứ năm</b><br/>" + dt.ToString("dd/MM/yyyy");
                case DayOfWeek.Friday:
                    return "<b>Thứ sáu</b><br/>" + dt.ToString("dd/MM/yyyy");
                case DayOfWeek.Saturday:
                    return "<b>Thứ bảy</b><br/>" + dt.ToString("dd/MM/yyyy");
                case DayOfWeek.Sunday:
                    return "<b>Chủ nhật</b><br/>" + dt.ToString("dd/MM/yyyy");
            }
            return "";
        }

        public static string getDaystringName(this DateTime dt)
        {
            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return "Thứ hai";
                case DayOfWeek.Tuesday:
                    return "Thứ ba";
                case DayOfWeek.Wednesday:
                    return "Thứ tư";
                case DayOfWeek.Thursday:
                    return "Thứ năm";
                case DayOfWeek.Friday:
                    return "Thứ sáu";
                case DayOfWeek.Saturday:
                    return "Thứ bảy";
                case DayOfWeek.Sunday:
                    return "Chủ nhật";
            }
            return "";
        }
        public static int GetWeeksInYear(int year)
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            DateTime date1 = new DateTime(year, 12, 31);
            Calendar cal = dfi.Calendar;
            return cal.GetWeekOfYear(date1, dfi.CalendarWeekRule,
                                                dfi.FirstDayOfWeek);
        }
        public static int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            var firstDate = new DateTime(year, 1, 4);
            while (firstDate.DayOfWeek != DayOfWeek.Monday)
                firstDate = firstDate.AddDays(-1);
            return firstDate.AddDays((weekOfYear - 1) * 7);
        }
        public static DateTime LastDateOfWeek(int year, int weekOfYear)
        {
            return FirstDateOfWeek(year, weekOfYear).AddDays(6);
        }
        #endregion

        public static string ToFileSize(this long size)
        {
            if (size < 1024)
            {
                return (size).ToString("F0") + " bytes";
            }
            else if (size < Math.Pow(1024, 2))
            {
                return (size / 1024).ToString("F0") + " KB";
            }
            else if (size < Math.Pow(1024, 3))
            {
                return (size / Math.Pow(1024, 2)).ToString("F0") + " MB";
            }
            else if (size < Math.Pow(1024, 4))
            {
                return (size / Math.Pow(1024, 3)).ToString("F0") + " GB";
            }
            else if (size < Math.Pow(1024, 5))
            {
                return (size / Math.Pow(1024, 4)).ToString("F0") + " TB";
            }
            else if (size < Math.Pow(1024, 6))
            {
                return (size / Math.Pow(1024, 5)).ToString("F0") + " PB";
            }
            else
            {
                return (size / Math.Pow(1024, 6)).ToString("F0") + " EB";
            }
        }

        public static ExpandoObject ToExpando(this object anonymousObject)
        {
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(anonymousObject))
            {
                var obj = propertyDescriptor.GetValue(anonymousObject);
                expando.Add(propertyDescriptor.Name, obj);
            }

            return (ExpandoObject)expando;
        }

        #region Seo
        public static String cut_String(String s, int sokt)
        {
            if (!String.IsNullOrEmpty(s) && s.Length > sokt)
            {
                while (s.Substring(sokt - 1, 1) != " " && sokt < s.Length - 1)
                {
                    sokt += 1;
                }
                s = s.Substring(0, sokt);
                s += "...";
            }
            return s;
        }
        public static bool IsNumeric(this string s)
        {
            float output;
            return float.TryParse(s, out output);
        }
        //replace html unclose tag
        public static string ToFriendlyUrl(string text)
        {
            for (int i = 33; i < 48; i++)
            {
                text = text.Replace(((char)i).ToString(), "");
            }

            for (int i = 58; i < 65; i++)
            {
                text = text.Replace(((char)i).ToString(), "");
            }

            for (int i = 91; i < 97; i++)
            {
                text = text.Replace(((char)i).ToString(), "");
            }
            for (int i = 123; i < 127; i++)
            {
                text = text.Replace(((char)i).ToString(), "");
            }
            text = text.Replace(" ", "-");
            Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
            string strFormD = text.Normalize(System.Text.NormalizationForm.FormD);
            return regex.Replace(strFormD, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        public static string ToFriendlyUrlSeo(string title)
        {
            if (title == null) return "";

            const int maxlen = 200;
            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (int i = 0; i < len; i++)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                    c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if ((int)c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxlen) break;
            }

            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            else
                return sb.ToString();
        }

        public static string ToFriendlyMax(string title, int maxlen)
        {
            if (title == null) return "";

            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (int i = 0; i < len; i++)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' ||
                    c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    {
                        sb.Append('-');
                        prevdash = true;
                    }
                }
                else if ((int)c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxlen) break;
            }

            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            else
                return sb.ToString();
        }

        public static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåąạậấầẫắằẵẳảaăẩ".Contains(s))
            {
                return "a";
            }
            else if ("èéêëęệếềệể".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïıịỉ".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőðơờớợốộồỗổởọ".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭůưứừựụủửữ".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿỹýỳỵ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if ("đđ".Contains(s))
            {
                return "d";
            }
            else if (c == 'ř')
            {
                return "r";
            }
            else if (c == 'ł')
            {
                return "l";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'Þ')
            {
                return "th";
            }
            else if (c == 'ĥ')
            {
                return "h";
            }
            else if (c == 'ĵ')
            {
                return "j";
            }
            else
            {
                return "";
            }
        }
        public static string ScrubHtml(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                var step1 = Regex.Replace(value, @"<[^>]+>|&nbsp;", "").Trim();
                var step2 = Regex.Replace(step1, @"\s{2,}", " ");
                return step2;
            }

            return value;
        }

        public static string convertToUnSign3(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
        public static string convertToUnSignChar(string s, string ch)
        {
            string stFormD = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }
            sb = sb.Replace('Đ', 'D');
            sb = sb.Replace('đ', 'd');
            sb = sb.Replace(" ", ch);
            return (sb.ToString().Normalize(NormalizationForm.FormD));
        }
        #endregion

        #region Xulyanh
        public static bool IsPptFileName(string name)
        {
            return name.ToLower().Contains(".ppt")
              || name.ToLower().Contains(".pptx");


        }
        public static bool IsImageFileName(string name)
        {
            return name.ToLower().Contains(".jpg")
              || name.ToLower().Contains(".png")
              || name.ToLower().Contains(".gif")
              || name.ToLower().Contains(".jpeg");

        }
        public static bool IsImage(this HttpPostedFileBase postedFile)
        {
            if (Path.GetExtension(postedFile.FileName).ToLower() == ".jpg"
               && Path.GetExtension(postedFile.FileName).ToLower() == ".png"
               && Path.GetExtension(postedFile.FileName).ToLower() == ".gif"
               && Path.GetExtension(postedFile.FileName).ToLower() == ".jpeg")
            {
                return true;
            }
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/gif" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".gif"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try
            {
                if (!postedFile.InputStream.CanRead)
                {
                    return false;
                }

                if (postedFile.ContentLength < ImageMinimumBytes)
                {
                    return false;
                }

                byte[] buffer = new byte[512];
                postedFile.InputStream.Read(buffer, 0, 512);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            //-------------------------------------------
            //  Try to instantiate new Bitmap, if .NET will throw exception
            //  we can assume that it's not a valid image
            //-------------------------------------------

            try
            {
                using (var bitmap = new Bitmap(postedFile.InputStream))
                {
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                postedFile.InputStream.Position = 0;
            }

            return true;
        }
        private static ImageCodecInfo GetEncoderInfo(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        public static void ResizeImageByte(string filePath, byte[] bs, int maxWidth, int maxHeight, int quality)
        {
            using (var ms = new MemoryStream(bs))
            {
                if (ms != null)
                {
                    try
                    {
                        Image image = Image.FromStream(ms);
                        int originalWidth = image.Width;
                        int originalHeight = image.Height;

                        float ratioX = (float)maxWidth / (float)originalWidth;
                        float ratioY = (float)maxHeight / (float)originalHeight;
                        float ratio = Math.Min(ratioX, ratioY);

                        int newWidth = originalWidth;
                        int newHeight = originalHeight;

                        if (originalWidth > maxWidth)
                        {
                            newWidth = (int)(originalWidth * ratio);
                            newHeight = (int)(originalHeight * ratio);
                        }
                        Bitmap newImage = null;
                        if (filePath.ToLower().Contains(".png"))
                        {
                            newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
                        }
                        else
                        {
                            newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
                        }

                        using (Graphics graphics = Graphics.FromImage(newImage))
                        {
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
                            graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                        }

                        ImageCodecInfo imageCodecInfo = GetEncoderInfo(image.RawFormat);

                        System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;

                        EncoderParameters encoderParameters = new EncoderParameters(1);

                        EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
                        encoderParameters.Param[0] = encoderParameter;
                        newImage.Save(filePath + "tmp", imageCodecInfo, encoderParameters);
                        newImage.Dispose();
                        image.Dispose();
                        File.Delete(filePath);
                        File.Move(filePath + "tmp", filePath);
                    }
                    catch
                    {

                    }
                }
            }
        }

        public static void DeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {

            }
        }

        public static void ResizeImage(string filePath, int maxWidth, int maxHeight, int quality)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //System.Threading.Thread.Sleep(500);
                    using (var image = new MagickImage(filePath))
                    {
                        if (image.Width < maxWidth && image.Height < maxHeight)
                        {
                            return;
                        }
                        var size = new MagickGeometry(maxWidth, maxHeight);
                        // This will resize the image to a fixed size without maintaining the aspect ratio.
                        // Normally an image will be resized to fit inside the specified size.
                        size.IgnoreAspectRatio = false;
                        image.Resize(size);
                        // Save the result
                        image.Write(filePath);
                    }

                }
                catch
                {
                }
            });
        }
        public static void ResizeThumbImage(string filePath, int maxWidth, int maxHeight)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //System.Threading.Thread.Sleep(500);
                    int idx = filePath.LastIndexOf(".");
                    string thumbPath = filePath.Substring(0, idx) + "_thumb" + filePath.Substring(idx);
                    File.Copy(filePath, thumbPath);
                    using (var image = new MagickImage(thumbPath))
                    {
                        if (image.Width < maxWidth && image.Height < maxHeight)
                        {
                            return;
                        }
                        var size = new MagickGeometry(maxWidth, maxHeight);
                        // This will resize the image to a fixed size without maintaining the aspect ratio.
                        // Normally an image will be resized to fit inside the specified size.
                        size.IgnoreAspectRatio = false;
                        image.Resize(size);
                        // Save the result
                        image.Write(thumbPath);
                    }

                }
                catch
                {
                }
            });
        }

        public static void ResizeCopyImage(string filePath, string copyPath, int maxWidth, int maxHeight, int quality)
        {
            System.Threading.Thread.Sleep(500);
            try
            {
                Image image = Image.FromFile(filePath);
                int originalWidth = image.Width;
                int originalHeight = image.Height;

                float ratioX = (float)maxWidth / (float)originalWidth;
                float ratioY = (float)maxHeight / (float)originalHeight;
                float ratio = Math.Min(ratioX, ratioY);

                int newWidth = originalWidth;
                int newHeight = originalHeight;

                if (originalWidth > maxWidth)
                {
                    newWidth = (int)(originalWidth * ratio);
                    newHeight = (int)(originalHeight * ratio);
                }
                Bitmap newImage = null;
                if (filePath.ToLower().Contains(".png"))
                {
                    newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
                }
                else
                {
                    newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format24bppRgb);
                }

                using (Graphics graphics = Graphics.FromImage(newImage))
                {
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                }

                ImageCodecInfo imageCodecInfo = GetEncoderInfo(image.RawFormat);

                System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;

                EncoderParameters encoderParameters = new EncoderParameters(1);

                EncoderParameter encoderParameter = new EncoderParameter(encoder, quality);
                encoderParameters.Param[0] = encoderParameter;
                newImage.Save(copyPath, imageCodecInfo, encoderParameters);
                newImage.Dispose();
                image.Dispose();
            }
            catch
            {

            }
        }
        //public static string GetFileExtension(this string fileName)
        //{
        //    string ext = string.Empty;
        //    int fileExtPos = fileName.LastIndexOf(".", StringComparison.Ordinal);
        //    if (fileExtPos >= 0)
        //        ext = fileName.Substring(fileExtPos, fileName.Length - fileExtPos);

        //    return ext;
        //}
        public static string GetFileExtension(this string fileName)
        {
            string ext = string.Empty;
            int fileExtPos = fileName.LastIndexOf(".", StringComparison.Ordinal);
            if (fileExtPos >= 0)
                ext = fileName.Substring(fileExtPos, fileName.Length - fileExtPos);

            return ext.Replace("\"", "").Replace(".", "");
        }
        #endregion

        #region Log
        public static void saveIP(string ip, string UserAgent, string uid, string fn)
        {
            //Task.Factory.StartNew(() =>
            //{
            using (DBEntities db = new DBEntities())
            {
                try
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    sys_web_acess wa = new sys_web_acess();
                    wa.from_device = getDecideNameAuto(UserAgent);
                    try
                    {
                        wa.from_ip = ip;
                    }
                    catch { wa.from_ip = ""; }
                    wa.is_time = DateTime.Now;
                    wa.full_name = fn;
                    wa.user_id = uid;
                    db.sys_web_acess.Add(wa);
                    db.SaveChanges();
                }
                catch { }
            }
            //});
        }

        public static void saveLog(string uid, string uname, string content, string control, string ip, string tokenid, string title, int loai, string module)
        {
            try
            {
                sys_logs os = new sys_logs();
                os.controller = control;
                os.log_content = content;
                os.log_date = DateTime.Now;
                os.modified_ip = ip;
                os.token_id = tokenid;
                os.full_name = uname;
                os.title = title;
                os.user_id = uid;
                os.is_type = loai;
                os.module = module;
                os.status = 0;
                saveLogs(os);
            }
            catch { }
        }

        public static void saveCMSLog(string uid, string content, string IDKey, string ip, string tokenid, string title, string module)
        {
            try
            {
                cms_logs os = new cms_logs();
                os.id_key = IDKey;
                os.log_content = content;
                os.created_date = DateTime.Now;
                os.created_ip = ip;
                os.created_token_id = tokenid;
                os.created_by = uid;
                os.modified_date = DateTime.Now;
                os.modified_ip = ip;
                os.modified_token_id = tokenid;
                os.modified_by = uid;
                os.log_title = title;
                os.log_module = module;
                if (wlog)
                {

                    using (DBEntities db = new DBEntities())
                    {
                        try
                        {
                            db.Configuration.LazyLoadingEnabled = false;
                            db.cms_logs.Add(os);
                            db.SaveChanges();
                        }
                        catch { }
                    }

                }
            }
            catch { }
        }

        public static void saveLogs(sys_logs ol)
        {
            if (wlog)
            {
                //Task.Factory.StartNew(() =>
                //{
                using (DBEntities db = new DBEntities())
                {
                    try
                    {
                        db.Configuration.LazyLoadingEnabled = false;
                        db.sys_logs.Add(ol);
                        db.SaveChanges();
                    }
                    catch { }
                }
                //});
            }
        }

        public static async Task<int> checkToken(sys_token t)
        {
            //-1 ko có token, 0 có token nhưng hết hạn,1 ok
            if (t == null)
            {
                return -1;
            }
            using (DBEntities db = new DBEntities())
            {
                var tk = await db.sys_token.FirstOrDefaultAsync(a => a.token_id == t.token_id && a.user_id == t.user_id);
                if (tk == null)
                {
                    return -1;
                }
                TimeSpan span = tk.date_end.Value - DateTime.Now;
                if (span.Minutes > timeout)
                {
                    return 0;
                }
                return 1;
            }
        }
        #endregion
        public static void saveLogFiles(string uid, int loai, int? file_id, string folder_id, string content, string ip, string tokenid)
        {
            try
            {
                file_log os = new file_log();
                os.file_id = file_id;
                os.folder_id = folder_id;
                os.contents = content;
                os.user_id = uid;
                os.log_type = loai;
                os.modified_ip = ip;
                os.created_date = DateTime.Now;
                os.created_ip = ip;
                os.date_view = DateTime.Now;
                os.created_token_id = tokenid;
                os.created_by = uid;
                os.modified_date = DateTime.Now;
                os.modified_ip = ip;
                os.modified_token_id = tokenid;
                os.modified_by = uid;
                if (wlog)
                {

                    using (DBEntities db = new DBEntities())
                    {
                        try
                        {
                            db.Configuration.LazyLoadingEnabled = false;
                            db.file_log.Add(os);
                            db.SaveChanges();
                        }
                        catch { }
                    }

                }
            }
            catch { }
        }

        public static int checkFileType(string fname)
        {
            string[] imageExtensions = {
                ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", //etc
                ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA", //etc
                ".AVI", ".MP4", ".DIVX", ".WMV", //etc
            };
            string[] videoExtensions = {
                ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA", //etc
                ".AVI", ".MP4", ".DIVX", ".WMV", //etc
            };
            if (imageExtensions.Contains(Path.GetExtension(fname), StringComparer.OrdinalIgnoreCase))
            {
                return 1;
            }
            else if (videoExtensions.Contains(Path.GetExtension(fname), StringComparer.OrdinalIgnoreCase))
            {
                return 3;
            }
            return 2;
        }
        #region Xử lý notification
        public static void SendNotification(List<string> ids, string ct, string name, string icon, string webview, string id, int type, int badge, object hub)
        {
            using (DBEntities db = new DBEntities())
            {
                string serverKey = "AAAAwWb3zRE:APA91bGW6K7O95XhFuXUW9XC_znVlcRnN5WaopEb-FbjEUI0-Xv-rW5g7mkT5QBzoZ1mXKv0s2QZYz2oepTJHJT8nSg20bEJ4Gll4AuKD9j1bM7ODVjg5jNLtX2HkLTwXNHAHy_0W4iO";
                string senderId = "830656204049";
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                //serverKey - Key from Firebase cloud messaging server  
                tRequest.Headers.Add(string.Format("Authorization: key={0}", serverKey));
                //Sender Id - From firebase project setting  
                tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
                tRequest.ContentType = "application/json";
                var serializer = new JavaScriptSerializer();
                var payload = new
                {
                    registration_ids = ids.ToArray(),
                    priority = "high",
                    content_available = true,
                    icon = icon,
                    notification = new
                    {
                        body = ct,
                        title = name,
                        sound = "sound.caf",
                        badge = badge
                    },
                    data = new
                    {
                        title = name,
                        message = ct,
                        image_url = icon,
                        view = webview,
                        key = id,
                        type = type,
                        click_action = "FLUTTER_NOTIFICATION_CLICK",
                        hub = serializer.Serialize(hub),
                    }
                };
                Byte[] byteArray = Encoding.UTF8.GetBytes(serializer.Serialize(payload));
                tRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    String sResponseFromServer = tReader.ReadToEnd();
                                }
                        }
                    }
                }
            }
        }
        #endregion

        // random code number
        public static string ranNumberFile()
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[6];
            provider.GetBytes(byteArray);
            var sessionId = BitConverter.ToString(byteArray, 0);
            return sessionId.Replace("-", "");
        }
        public static void saveNotify(string user_send, string receiver, string icon, string title, string contents, string note, int type, int is_type, bool seen, string module_key, string id_key, string group_id, string token_id, string created_token_id, string add)
        {
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    sys_sendhub noty = new sys_sendhub();
                    noty.senhub_id = helper.GenKey();
                    noty.user_send = user_send;
                    noty.receiver = receiver;
                    noty.icon = icon;
                    noty.title = title;
                    noty.contents = contents;
                    noty.note = note;
                    noty.type = type;
                    noty.is_type = is_type;
                    noty.seen = seen;
                    noty.module_key = module_key;
                    noty.id_key = id_key;
                    noty.group_id = group_id;
                    noty.token_id = token_id;
                    noty.date_send = DateTime.Now;
                    noty.created_by = user_send;
                    noty.created_date = DateTime.Now;
                    noty.created_ip = add;
                    noty.created_token_id = created_token_id;
                    db.sys_sendhub.Add(noty);
                    db.SaveChanges();

                    #region SendSocket
                    //send socket
                    List<string> users = new List<string>();
                    users.Add(receiver);
                    var message = new Dictionary<string, dynamic>
                        {
                            { "event", "sendNotify" },
                        {    "user_id", user_send },
                            { "title", title },
                            { "contents", contents },
                            { "date", DateTime.Now },
                            { "uids", users },
                        };
                    if (helper.socketClient != null && helper.socketClient.Connected == true)
                    {
                        try
                        {
                            helper.socketClient.EmitAsync("sendData", message);
                        }
                        catch { };
                    }
                    #endregion
                }
            }
            catch { }
        }

        #region Send noti chat
        public static void send_noti_chat(string user_send, string id_key, string group_id, List<string> users, string title, string content, int is_type, string ip, string token_id)
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    using (DBEntities db = new DBEntities())
                    {
                        #region Sendhub
                        List<sys_sendhub> sendhubs = new List<sys_sendhub>();
                        foreach (String user_id in users)
                        {
                            var sh = new sys_sendhub();
                            sh.senhub_id = helper.GenKey();
                            sh.title = title;
                            sh.id_key = id_key;
                            sh.group_id = group_id;
                            sh.user_send = user_send;
                            sh.created_by = user_send;
                            sh.created_date = DateTime.Now;
                            sh.created_ip = ip;
                            sh.created_token_id = token_id;
                            sh.receiver = user_id;
                            sh.type = 4;
                            sh.is_type = is_type;
                            sh.module_key = "M8";
                            sh.token_id = token_id;
                            sh.contents = content;
                            sh.date_send = DateTime.Now;
                            sh.seen = false;
                            sendhubs.Add(sh);
                        }
                        if (sendhubs.Count > 0)
                        {
                            db.sys_sendhub.AddRange(sendhubs);
                            await db.SaveChangesAsync();
                        }
                        #endregion
                        #region SendSocket
                        //send socket
                        var message = new Dictionary<string, dynamic>
                        {
                            { "event", "sendNotify" },
                            { "user_id", user_send },
                            { "title", title },
                            { "contents", content },
                            { "date", DateTime.Now },
                            { "uids", users },
                        };
                        if (helper.socketClient != null && helper.socketClient.Connected == true)
                        {
                            try
                            {
                                await helper.socketClient.EmitAsync("sendData", message);
                            }
                            catch { };
                        }
                        #endregion
                    }
                }
                catch { }
            });
        }
        #endregion

        #region Send popup noti
        public static void send_popup_noti(string user_send, string title, string content, List<string> users, string url_img, string url_link)
        {
            using (DBEntities db = new DBEntities())
            {
                #region SendSocket
                //send socket
                var message = new Dictionary<string, dynamic>
                        {
                            { "event", "sendNotify" },
                            { "user_id", user_send },
                            { "title", title },
                            { "contents", content },
                            { "date", DateTime.Now },
                            { "uids", users },
                            { "image", url_img },
                            { "url", url_link },
                        };
                if (helper.socketClient != null && helper.socketClient.Connected == true)
                {
                    try
                    {
                        helper.socketClient.EmitAsync("sendData", message);
                    }
                    catch { };
                }
                #endregion
            }
        }
        #endregion

        #region Check SQL Injection
        public static Boolean checkForSQLInjection(string userInput)
        {
            bool isSQLInjection = false;
            string[] sqlCheckList = { "--",
                                      ";--",
                                      ";",
                                      "/*",
                                      "*/",
                                      "@@",
                                      "@",
                                      "char",
                                      "nchar",
                                      "varchar",
                                      "nvarchar",
                                      "alter",
                                      "begin",
                                      "cast",
                                      "create",
                                      "cursor",
                                      "declare",
                                      "delete",
                                      "drop",
                                      "end",
                                      "exec",
                                      "execute",
                                      "fetch",
                                      "insert",
                                      "kill",
                                      "select",
                                      "sys",
                                      "sysobjects",
                                      "syscolumns",
                                      "table",
                                      "update"
                                    };
            string CheckString = userInput.Replace("'", "''");
            for (int i = 0; i <= sqlCheckList.Length - 1; i++)
            {
                if ((CheckString.IndexOf(sqlCheckList[i], StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    isSQLInjection = true;
                }
            }
            return isSQLInjection;
        }
        #endregion

        public static string strConnect(string conn)
        {
            string de_str = Codec.DecryptString(conn, keyConnect);
            return de_str;
        }

        public static void UploadFileToDestination(string jwtcookie, string root, MultipartFileData fileData, string newFileName, int? rswidth, int? rsheight) // rswidth, rsheight: size thumb image
        {
            Upload upload = new Upload();
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    await upload.UpdateFile(jwtcookie, root, fileData, newFileName, rswidth, rsheight);
                    if (System.IO.File.Exists(fileData.LocalFileName))
                    {
                        System.IO.File.Delete(fileData.LocalFileName);
                    }
                    if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["Portals"]))
                    {
                        if (System.IO.File.Exists(root + newFileName))
                        {
                            System.IO.File.Delete(root + newFileName);
                        }
                    }
                }
                catch { }
            });
        }
        #region encode data proc 
        public class MemoryCacheHelper
        {
            public static string GetValue(string key)
            {
                return (string)MemoryCache.Default.Get(key);
            }
            public static bool Add(string key, object value, DateTimeOffset absExpiration)
            {
                return MemoryCache.Default.Add(key, value, absExpiration);
            }
            public static void Set(string key, object value, DateTimeOffset absExpiration)
            {
                MemoryCache.Default.Set(key, value, absExpiration);


            }
            public static void Delete(string key)
            {
                MemoryCache memoryCache = MemoryCache.Default;
                if (memoryCache.Contains(key))
                {
                    memoryCache.Remove(key);
                }
            }
        }
        public static string GenerateRandomKey()
        {
            const int keyLength = 16;
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rng = new RNGCryptoServiceProvider();
            var result = new char[keyLength];
            var bytes = new byte[sizeof(uint)];

            for (int i = 0; i < keyLength; i++)
            {
                rng.GetBytes(bytes);
                uint value = BitConverter.ToUInt32(bytes, 0);
                result[i] = allowedChars[(int)(value % (uint)allowedChars.Length)];
            }

            return new string(result);
        }
        public static (string, string) checkEncodeData(string JSONresult, string user_id, string EncriptKey)
        {
            string key_code = helper.psKey;
            if (helper.isEncodeProc)
            {
                using (DBEntities db = new DBEntities())
                {
                    // key_code = db.sys_users.FirstOrDefault(e => e.user_id == user_id).key_encode_data;
                    //  key_code = MemoryCacheHelper.GetValue(user_id).ToString();
                    key_code = MemoryCacheHelper.GetValue(user_id);
                    if (key_code == null)
                    {
                        key_code = db.sys_users.FirstOrDefault(e => e.user_id == user_id).key_encode_data;
                        if (key_code != null) MemoryCacheHelper.Add(user_id, key_code, DateTimeOffset.UtcNow.AddMinutes(30));
                    }
                    if (key_code == null) key_code = helper.psKey;
                    JSONresult = Codec.EncryptString(JSONresult, key_code);
                }
            }
            string data_Key = Codec.EncryptString((helper.isEncodeProc ? "1111" : "0000") + "26$#" + key_code, EncriptKey);

            return (JSONresult, data_Key);
        }
        #endregion
        public static Nullable<int> Department(IEnumerable<Claim> claims)
        {
            string ctid = claims.Where(p => p.Type == "dept").FirstOrDefault()?.Value;
            if (ctid != null)
                return Convert.ToInt32(ctid);
            else return null;
        }
        public static Nullable<int> OrgainzationChild(IEnumerable<Claim> claims)
        {
            string ctid = claims.Where(p => p.Type == "ctid").FirstOrDefault()?.Value;
            if (ctid != null)
                return Convert.ToInt32(ctid);
            else return null;
        }
        public static Nullable<int> Orgainzation(IEnumerable<Claim> claims)
        {
            string ctid = claims.Where(p => p.Type == "dvid").FirstOrDefault()?.Value;
            if (ctid != null)
                return Convert.ToInt32(ctid);
            else return null;
        }
    }
}