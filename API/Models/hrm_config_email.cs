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
    
    public partial class hrm_config_email
    {
        public int config_email_id { get; set; }
        public string email_address { get; set; }
        public string email_pasw { get; set; }
        public string incoming_mails { get; set; }
        public string outgoing_mails { get; set; }
        public string port { get; set; }
        public Nullable<int> time_set { get; set; }
        public string url_content { get; set; }
        public string display_name { get; set; }
        public string default_content { get; set; }
        public string key_encript { get; set; }
        public Nullable<int> organization_id { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string modified_ip { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string created_ip { get; set; }
    }
}
