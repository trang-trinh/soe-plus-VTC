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
    
    public partial class hrm_profile_edit
    {
        public int key_id { get; set; }
        public string profile_id { get; set; }
        public string profile_code { get; set; }
        public Nullable<int> place_register_permanent { get; set; }
        public Nullable<int> identity_papers_id { get; set; }
        public string identity_papers_code { get; set; }
        public Nullable<System.DateTime> identity_date_issue { get; set; }
        public Nullable<int> identity_place_id { get; set; }
        public Nullable<System.DateTime> identity_papers_outdate { get; set; }
        public Nullable<int> nationality_id { get; set; }
        public Nullable<int> marital_status { get; set; }
        public string tax_code { get; set; }
        public string bank_number { get; set; }
        public string bank_account { get; set; }
        public Nullable<int> bank_id { get; set; }
        public string bike_code { get; set; }
        public string car_code { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string place_permanent { get; set; }
        public string place_residence { get; set; }
        public string involved_name { get; set; }
        public string involved_phone { get; set; }
        public string involved_place { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> is_flag { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_ip { get; set; }
        public string modified_token_id { get; set; }
    
        public virtual hrm_profile hrm_profile { get; set; }
    }
}
