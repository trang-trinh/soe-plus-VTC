﻿using System;
using System.Collections.Generic;

namespace API.Models
{



    public class docReport
    {
        public DateTime timeExport { get; set; }
       
        public string user_id { get; set; }
        public string user_name { get; set; }
        public int report_type { get; set; }
        public int page_no { get; set; }
        public int page_size { get; set; }
        public DateTime? start_dateI { get; set; }
        public DateTime? end_dateI { get; set; }
        public DateTime? start_dateD { get; set; }
        public DateTime? end_dateD { get; set; }
        public string search { get; set; }
        public string user_recever { get; set; }
        public string field_id { get; set; }
        public string dispatch_book_id { get; set; }
        public string doc_group_id { get; set; }
        public string department_id_process { get; set; }
        public string department_id { get; set; }
   
    }
    public class colExcel
    {
        public string title { get; set; }
        public int Colspan { get; set; }
        public int Rowspan { get; set; }
        public int? FontSize { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string BgColor { get; set; }
        public string Color { get; set; }
        public bool Bold { get; set; }
        public string Align { get; set; }
        public int? Padding { get; set; }
    }
    public class modelExcel
    {
        public string title { get; set; }
        public string excelname { get; set; }
        public List<colExcel> header { get; set; }
        public List<List<colExcel>> rows { get; set; }
    }
    public class sqlPar
    {
        public string par { get; set; }
        public string va { get; set; }
    }
    public class stringToken
    {
        public string strToken { get; set; }

    }
    public class PDFOpition
    {
        public string orientation { get; set; }
        public string pageSize { get; set; }
        public double left { get; set; }
        public double bottom { get; set; }
        public double top { get; set; }
        public double right { get; set; }
    }
    public class modelHTML
    {
        public string name { get; set; }
        public string html { get; set; }
        public string lib { get; set; }
        public PDFOpition opition { get; set; }
    }

    public class sqlProc
    {
        public string excelname { get; set; }
        public string proc { get; set; }
        public List<sqlPar> par { get; set; }
        public bool query { get; set; }
    }
    public class sqlPublicProc
    {
        public string excelname { get; set; }
        public string proc { get; set; }
        public List<sqlPar> par { get; set; }
        public string publictoken { get; set; }
    }

    public class status
    {
        public int IntID { get; set; }
        public string TextID { get; set; }
        public int Intstatus { get; set; }
        public bool Bitstatus { get; set; }
    }
    public class FilterSQL
    {
        public string sqlS { get; set; }
        public string sqlF { get; set; }
        public string sqlO { get; set; }
        public string Search { get; set; }
        public string id { get; set; }
        public bool next { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public DateTime Date { get; set; }
        public List<FieldSQL> fieldSQLS { get; set; }
    }
    public class FieldSQL
    {
        public string key { get; set; }
        public string filteroperator { get; set; }
        public List<constraints> filterconstraints { get; set; }
        public int type_of { get; set; } //null: key in (key), 1: key in (name) , 
    }
    public class constraints
    {
        public string value { get; set; }
        public string matchMode { get; set; }
    }
    public class settings
    {
        public bool socket { get; set; }
        public bool debug { get; set; }
        public string logCongtent { get; set; }
        public bool wlog { get; set; }
        public int milisec { get; set; }
        public int timeout { get; set; }
        public int cache { get; set; }
        public string publictoken { get; set; }
        public int wrongAcceptPass { get; set; }
        public int wrongAcceptIP { get; set; }
        public bool isBlockIP { get; set; }
        public string pathBlockIP { get; set; }
        public string apiBHBQP { get; set; }
        public string tokenBHBQP { get; set; }
        public string keycodeBHBQP { get; set; }
        public string path_xml { get; set; }
        public string socketUrl { get; set; }
        public string fileNameSettingApp { get; set; }
        public string filePathSettingApp { get; set; }
        public string email { get; set; }
        public string psemail { get; set; }
        public int timemail { get; set; }

        public string userftp { get; set; }
        public string psftp { get; set; }
        public bool isEncodeProc { get; set; }
        public string docOnlineUrl { get; set;}

    }
    public class connectString
    {
        public string DataSource { get; set; }
        public string InitialCatalog { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
    }
    public class groupName
    {

        public string groupNameOld { get; set; }

        public string groupNameNew { get; set; }

    }
    public class userReceiver
    {

        public string user_receiver { get; set; }

        public int organization_id { get; set; }

    }
    public class Trangthai
    {
        public int IntID { get; set; }
        public string TextID { get; set; }
        public int IntTrangthai { get; set; }
        public bool BitTrangthai { get; set; }
        public bool check { get; set; }
    }
    public class modelTask_Test
    {
        public task_test task_Test { get; set; }
        public List<api_bug> api_Bugs { get; set; }

    }
    public class IsVisitor
    {
        public int IntID { get; set; }
        public string TextID { get; set; }

        public string user_id { get; set; }

    }
    public class Ismain
    {
        public int IntID { get; set; }
        public string TextID { get; set; }

        public bool BitMain { get; set; }

    }

    public class BlockIP
    {
        public string IP { get; set; }
        public int Count { get; set; }
    }
    public class MailInfo
    {
        public string to { get; set; }
        public string display_name { get; set; }
        public string subject { get; set; }

        public string body { get; set; }
        public bool isBodyHtml { get; set; }

    }
    public class infoMailSend
    {
        public string to { get; set; }
        public string email { get; set; }
        public string display_name { get; set; }
        public string subject { get; set; }
        public string top { get; set; }
        public string body { get; set; }
        public string bottom { get; set; }
        public bool isBodyHtml { get; set; }

    }
    public class popupNoti
    {
        public List<string> uids { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string image { get; set; }
        public string tag { get; set; }
        public string url { get; set; }
    }

    public class MappingFile
    {
        public string file_key_id { get; set; }
        public string path_file { get; set; }
        public int type_save_file { get; set; }
    }
}