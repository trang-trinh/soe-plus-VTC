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
    
    public partial class calendar_sign
    {
        public string sign_id { get; set; }
        public string calendar_id { get; set; }
        public string calendar_duty_id { get; set; }
        public string procedure_id { get; set; }
        public string sign_name { get; set; }
        public int is_step { get; set; }
        public Nullable<int> is_type { get; set; }
        public Nullable<bool> is_sign { get; set; }
        public Nullable<bool> is_skip { get; set; }
        public Nullable<bool> status { get; set; }
        public Nullable<bool> is_close { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_ip { get; set; }
        public string modified_token_id { get; set; }
        public Nullable<int> organization_id { get; set; }
    }
}
