//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace API.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class device_process
    {
        public int device_process_id { get; set; }
        public string device_process_code { get; set; }
        public int device_note_id { get; set; }
        public Nullable<int> approved_group_id { get; set; }
        public string content { get; set; }
        public string returned_content { get; set; }
        public Nullable<bool> is_type { get; set; }
        public Nullable<bool> is_approved { get; set; }
        public Nullable<bool> is_view { get; set; }
        public Nullable<System.DateTime> date_view { get; set; }
        public Nullable<System.DateTime> date_approved { get; set; }
        public Nullable<System.DateTime> date_processing { get; set; }
        public Nullable<int> device_process_type { get; set; }
        public System.DateTime date_send { get; set; }
        public string approved_user_id { get; set; }
        public string status { get; set; }
        public Nullable<bool> is_returned { get; set; }
        public Nullable<int> organization_id { get; set; }
        public Nullable<int> is_order { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_token_id { get; set; }
        public string modified_ip { get; set; }
        public Nullable<bool> is_last { get; set; }
        public Nullable<int> pre_process_id { get; set; }
    }
}
