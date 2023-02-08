using API.Models;
using DEMO_FULL_CA_DOTNET_WEB.Models;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ViettelFileSigner;

namespace API.Helper
{
    public class ImpMobileCA
    {
        static SignPdfFile pdfSig;
        string certViettelCABase64 = "MIIEKDCCAxCgAwIBAgIKYQ4N5gAAAAAAETANBgkqhkiG9w0BAQUFADB+MQswCQYDVQQGEwJWTjEzMDEGA1UEChMqTWluaXN0cnkgb2YgSW5mb3JtYXRpb24gYW5kIENvbW11bmljYXRpb25zMRswGQYDVQQLExJOYXRpb25hbCBDQSBDZW50ZXIxHTAbBgNVBAMTFE1JQyBOYXRpb25hbCBSb290IENBMB4XDTE1MTAwMjAyMzIyMFoXDTIwMTAwMjAyNDIyMFowOjELMAkGA1UEBhMCVk4xFjAUBgNVBAoTDVZpZXR0ZWwgR3JvdXAxEzARBgNVBAMTClZpZXR0ZWwtQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDLdiGZcPhwSm67IiLUWELaaol8kHF+qHPmEdcG0VDKf0FtpSWiE/t6NPzqqmoF4gbIrue1/TzUs7ZeAj28o6Lb2BllA/zB6YFrXfppD4jKqHMO139970MeTbDrhHTbVugX4t2QHS+B/p8+8lszJpuduBrnZ/LWxbhnjeQRr21g89nh/W5q1VbIvZnq4ci5m0aDiJ8arhK2CKpvNDWWQ5E0L7NTVoot8niv6/Wjz19yvUCYOKHYsq97y7eBaSYmpgJosD1VtnXqLG7x4POdb6Q073eWXQB0Sj1qJPrXtOqWsnnmzbbKMrnjsoE4gg9B6qLyQS4kRMp0RrUV0z041aUFAgMBAAGjgeswgegwCwYDVR0PBAQDAgGGMBIGA1UdEwEB/wQIMAYBAf8CAQAwHQYDVR0OBBYEFAhg5h8bFNlIgAtep1xzJSwgDfnWMB8GA1UdIwQYMBaAFM1iceRhvf497LJAYNOBdd06rGvGMDwGA1UdHwQ1MDMwMaAvoC2GK2h0dHA6Ly9wdWJsaWMucm9vdGNhLmdvdi52bi9jcmwvbWljbnJjYS5jcmwwRwYIKwYBBQUHAQEEOzA5MDcGCCsGAQUFBzAChitodHRwOi8vcHVibGljLnJvb3RjYS5nb3Yudm4vY3J0L21pY25yY2EuY3J0MA0GCSqGSIb3DQEBBQUAA4IBAQCHtdHJXudu6HjO0571g9RmCP4b/vhK2vHNihDhWYQFuFqBymCota0kMW871sFFSlbd8xD0OWlFGUIkuMCz48WYXEOeXkju1fXYoTnzm5K4L3DV7jQa2H3wQ3VMjP4mgwPHjgciMmPkaBAR/hYyfY77I4NrB3V1KVNsznYbzbFtBO2VV77s3Jt9elzQw21bPDoXaUpfxIde+bLwPxzaEpe7KJhViBccJlAlI7pireTvgLQCBzepJJRerfp+GHj4Z6T58q+e3a9YhyZdtAHVisWYQ4mY113K1V7Z4D7gisjbxExF4UyrX5G4W0h0gXAR5UVOstv5czQyDraTmUTYtx5J";
        string priKey = "MIICdwIBADANBgkqhkiG9w0BAQEFAASCAmEwggJdAgEAAoGBAI1ZGghrDQryVQcDDe1gpJ1TQvRWZz0P59/04LlMOdcf0G7XGHy7tzmWffdKgVmlkTbw0tMGa37lg7Suxrm9JpmhcEQmTMo6L06SEmBYSiZfPWMyDnjdWhE8mPMP3ju3xx24UVXcpSclykQflTBr42yZgqsgM19ndsDFkwtz2uWDAgMBAAECgYAJzOPBMar111eN5OhSTSEcx2kdB+Cgmzm4jYIHVwGrqMkK5l8MRvetRoH1Y3UUgiZPaOM1Pny1j7RSEsw0lKjYY4Jawm5n7js13VkIs9tO8HhK00Oo/7a6ZRxAbczpfvGHmMdwaUQgHSGngzE7T3D8Eh4xx3Qu6fmTAIeKPNSMAQJBANfPb9gDkWIsQ/16siOQaTEfacASx/2MvucfrQ2WYGWbG1xNVfA1hkC2tmRRu3SRJp/1lhlERTvOSac4m9IBMasCQQCnq75nAlQTU+/1GvH8nLyEPrCudn40jMCKSEkMWJKKVuiKCrF2GJCZQipNs1DfMSyPggux3Z3hQ62JBuZfNvOJAkEAxIYCM5QMMHpe79Vrozc+k50nj+GKfTpOHeqajGUEI4K7x7IlMDmNqCC6t2A2dFA5/DCIHzosUeno6H6EZxjvQQJAY+IStgiUD0OEge4AU+0G/HzgAb5C5okmtfnj0j/9Y/3r3zgJiYGOuk3JJ6p3tc30brUYxGdyAtyvRx7eI8B3iQJBAJpa4qW6sJ36AKZFLq4D6EwaL2G3kc1bVFSwgRB0TFMB3Vak4O4mu1HWfgCWo20RvJCfcYCrIEdguvd3IunQ9Mc=";//Khai bao gia tri Prikey 
        string apID = "AP2";//Khai bao AppId

        public static String HASH_ALGORITHM_SHA_1 = "SHA1";
        public static String HASH_ALGORITHM_SHA_256 = "SHA256";
        internal string SignMobileCA(string filePath, string fileName, string phoneNumber, string lydo, string diadiem, bool showKyboi)
        {
            try
            {
                using (DBEntities db = new DBEntities())
                {
                    if (fileName == null || fileName.Trim().Length == 0 || fileName.IndexOf(".pdf") == -1)
                    {
                        return "Tên file trống";
                    }

                    //string pattern = @"[^-_.A-Za-z0-9]";
                    //Match m = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);
                    //if (m.Success)
                    //{
                    //    return Json(new { error = 1, ms = "Tên file chứa ký tự đặc biệt" }, JsonRequestBehavior.AllowGet);
                    //}

                    String fileFullPath = filePath + "/" + fileName;
                    bool exists = System.IO.File.Exists(fileFullPath);
                    if (!exists)
                    {
                        return "File không tồn tại";
                    }

                    string[] stringSeparators = new string[] { ".pdf" };
                    String[] chainBase64 = fileName.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    String name = fileName.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries)[0] + "_casigned";
                    String ext = "pdf";

                    exists = System.IO.File.Exists(filePath + "/" + name + "." + ext);
                    if (exists)
                    {
                        int index = 1;
                        String name_2 = name + "_" + index;
                        String path = filePath + "/" + name_2 + "." + ext;
                        while (System.IO.File.Exists(path))
                        {
                            index++;
                            name_2 = name + "_" + index;
                            path = filePath + "/" + name_2 + "." + ext;
                        }
                        name = name_2;
                    }
                    string signedFile = filePath + "/" + name + "." + ext;

                    // Contructor
                    PdfSignerSynchronous signer = new PdfSignerSynchronous();
                    //System.Security.Cryptography.X509Certificates.X509Certificate[] chain = UtilSigner.GetChainCertFromP12(textBoxFileP12.Text, textBoxPasswordFileP12.Text);

                    MobileCA mobileCA = new MobileCA();

                    string msisdn = phoneNumber;
                    bool userSelectCert = true;
                    string certSerial = null;
                    if (!userSelectCert)
                    {
                        certSerial = "08276d4f2979228e";
                    }

                    string dataDisplay = "SmartOffice";


                    //X509Certificate[] chain = mobileCA.GetCertificate(apID, msisdn, certSerial, fileP12, passFileP12);

                    String[] certList = mobileCA.GetCertificatefromPrikey(apID, msisdn, certSerial, priKey);

                    var certParser = new Org.BouncyCastle.X509.X509CertificateParser();
                    Org.BouncyCastle.X509.X509Certificate x509Cert = certParser.ReadCertificate(Convert.FromBase64String(certList[0]));

                    Org.BouncyCastle.X509.X509Certificate[] certChain = null;
                    X509Certificate certViettelCA = CertUtils.GetX509Cert(certViettelCABase64);
                    if (certViettelCA != null)
                    {
                        certChain = new X509Certificate[] { x509Cert, certViettelCA };
                    }

                    if (certChain.Length != 2)
                    {
                        return "Lấy Chứng thư số không thành công. Không lấy được CTS CA.";
                    }

                    // Set parameters
                    signer.SigTextFormat = PdfSignerSynchronous.FORMAT_TEXT_4;
                    signer.SigContact = UtilSigner.GetCNFromDN(x509Cert.SubjectDN.ToString());
                    signer.SigLocation = "Hanoi";
                    signer.IsMultiSignatures = true;
                    signer.Visible = true;
                    signer.OriginX = 10;
                    signer.OriginY = 10;
                    signer.CoordinateX = 300;
                    signer.CoordinateY = 50;
                    signer.TsaClient = null;
                    signer.UseTSA = false;
                    DateTime signDate = DateTime.Now;

                    // Create hash file

                    //Khai bao duong dan toi file pdf can ky tren web server
                    //string fileFullPath = @"D:\backup\Project\Viettel CA\Ho Tro\Dot Net\Demo Mobile CA ASP DotNet\PDF\Sample.pdf";
                    //ext = DateTime.Now.Ticks.ToString();
                    //string signedFile = @"D:\backup\Project\Viettel CA\Ho Tro\Dot Net\Demo Mobile CA ASP DotNet\PDF\Sample_" + ext + ".pdf";
                    //byte[] hash = signer.CreateHash(fileFullPath, signedFile, certChain, signDate);
                    string base64Hash = GetHashTypeRectangleText(fileFullPath, certChain, lydo, diadiem, showKyboi);
                    byte[] hash = Convert.FromBase64String(base64Hash);
                    // Sign hash use Prikey

                    string signature = mobileCA.signSynchronouswithPrikey(base64Hash, apID, msisdn, dataDisplay, priKey);
                    //byte[] signature = mobileCA.signSynchronouswithPrikey(base64Hash, apID, msisdn, dataDisplay, priKey);
                    //byte[] signature = mobileCA.signSynchronouswithPrikey(hash, apID, msisdn, dataDisplay, priKey);

                    if (signature == null)
                    {
                        return "Phát sinh lỗi trong quá trình thực hiện chữ ký số";
                    }

                    //var session = System.Web.HttpContext.Current.Session;

                    //if (session["pdfSig"] != null)
                    //{
                    //    try
                    //    {
                    //        pdfSig = session["pdfSig"] as SignPdfFile;
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        return "Không tìm thấy phiên làm việc";
                    //    }

                    //}
                    if (pdfSig == null)
                    {
                        return "Không tìm thấy phiên làm việc";
                    }
                    TimestampConfig timestampConfig = new TimestampConfig();
                    timestampConfig.UseTimestamp = false;
                    //string signatureBase64 = Convert.ToBase64String(signature);
                    if (!pdfSig.insertSignature(signature, signedFile, timestampConfig))
                    {
                        return "Chèn chữ ký số vào trong file lỗi";
                    }
                    //else
                    //{
                    //    ViewBag.statusResultSignMobileCA = "success";
                    //    ViewBag.signedFileName = name + "." + ext;
                    //}

                    //// Insert signature into file
                    //if (signer.InsertSignature(signature))
                    //{
                    //    ViewBag.statusResultSignMobileCA = "success";
                    //    ViewBag.signedFileName = name + "." + ext;
                    //}
                    //else
                    //{
                    //    ViewBag.descErrorSignMobileCA = "Phát sinh lỗi khi chèn chữ ký số vào file văn bản";
                    //    return View();
                    //}

                    return "OK";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public static String GetHashTypeRectangleText(String src, X509Certificate[] certChain, string lydo, string diadiem, bool showKyboi)
        {
            SignPdfFile imp_pdfSig = new SignPdfFile();
            string formatText = "";
            if (lydo.Trim() == "undefined") lydo = "";
            if (diadiem.Trim() == "undefined") diadiem = "";
            if (showKyboi)
            {
                if (string.IsNullOrEmpty(lydo) && string.IsNullOrEmpty(diadiem))
                {
                    formatText = PdfSignerSynchronous.FORMAT_TEXT_10;
                }
                else if (string.IsNullOrEmpty(lydo))
                {
                    formatText = PdfSignerSynchronous.FORMAT_TEXT_9;
                }
                else if (string.IsNullOrEmpty(diadiem))
                {
                    formatText = PdfSignerSynchronous.FORMAT_TEXT_8;
                }
                else
                {
                    formatText = PdfSignerSynchronous.FORMAT_TEXT_7;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(lydo) && string.IsNullOrEmpty(diadiem))
                {
                    formatText = "";
                }
                else if (string.IsNullOrEmpty(lydo))
                {
                    formatText = PdfSignerSynchronous.FORMAT_TEXT_13;
                }
                else if (string.IsNullOrEmpty(diadiem))
                {
                    formatText = PdfSignerSynchronous.FORMAT_TEXT_12;
                }
                else
                {
                    formatText = PdfSignerSynchronous.FORMAT_TEXT_11;
                }
            }
            DisplayConfig displayConfig = DisplayConfig.generateDisplayConfigRectangleText(1, 450, 760, 150, 80,
                    null, formatText, CertUtils.GetCN(certChain[0]), lydo, diadiem, DisplayConfig.DATE_FORMAT_1);
            String base64Hash = imp_pdfSig.createHash(src, certChain, displayConfig);
            //var session = System.Web.HttpContext.Current.Session;
            //session["pdfSig"] = pdfSig;
            pdfSig = imp_pdfSig;
            return base64Hash;
        }

        public static String GetHashTypeRectangleText2_ExistedSignatureField(String src, X509Certificate[] certChain, String displayText, String fieldName)
        {
            SignPdfFile imp_pdfSig = new SignPdfFile();
            //DisplayConfig displayConfig = DisplayConfig.generateDisplayConfigRectangleText(1, 10, 10, 200, 80,
            //        DisplayConfig.SIGN_TEXT_FORMAT_4, "Dương Ngọc Khánh", "Kiểm tra", "Hà Nội", DisplayConfig.DATE_FORMAT_1);
            DisplayConfig displayConfig = DisplayConfig.generateDisplayConfigRectangleText_ExistedSignatureField(1, 10, 10, 200, 80,
                    displayText, null, CertUtils.GetCN(certChain[0]), "Kiểm tra", "Hà Nội", DisplayConfig.DATE_FORMAT_1);
            String base64Hash = imp_pdfSig.createHashExistedSignatureField(src, certChain, displayConfig, fieldName);
            //var session = System.Web.HttpContext.Current.Session;
            //session["pdfSig"] = pdfSig;
            pdfSig = imp_pdfSig;
            return base64Hash;
        }
    }
}