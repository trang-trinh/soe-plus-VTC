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
    
    public partial class hrm_ca_type_decision
    {
        public int type_decision_id { get; set; }
        public string type_decision_name { get; set; }
        public string type_decision_code { get; set; }
        public Nullable<int> organization_id { get; set; }
        public Nullable<int> is_order { get; set; }
        public Nullable<bool> is_multiple { get; set; }
        public Nullable<bool> status { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_token_id { get; set; }
        public string modified_ip { get; set; }
        public Nullable<bool> is_system { get; set; }
        public string content { get; set; }
        public Nullable<int> report_key { get; set; }
    }
}