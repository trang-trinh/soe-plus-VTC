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
    
    public partial class sys_organization_history
    {
        public int organization_history_id { get; set; }
        public Nullable<int> organization_id { get; set; }
        public string decision_number { get; set; }
        public string content { get; set; }
        public Nullable<int> organization_type { get; set; }
        public string organization_name { get; set; }
        public string organization_name_en { get; set; }
        public string short_name { get; set; }
        public Nullable<System.DateTime> foundation_date { get; set; }
        public Nullable<System.DateTime> dissolution_date { get; set; }
        public string representative { get; set; }
        public string business_code { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string address_registration { get; set; }
        public string is_url { get; set; }
        public string mail { get; set; }
        public string fax { get; set; }
        public string feature { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_token_id { get; set; }
        public string modified_ip { get; set; }
        public Nullable<System.DateTime> decision_date { get; set; }
    }
}
