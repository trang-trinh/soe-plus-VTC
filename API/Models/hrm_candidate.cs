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
    
    public partial class hrm_candidate
    {
        public int candidate_id { get; set; }
        public string candidate_code { get; set; }
        public Nullable<int> campaign_id { get; set; }
        public Nullable<int> candidate_source { get; set; }
        public string candidate_name { get; set; }
        public string candidate_avatar { get; set; }
        public Nullable<System.DateTime> candidate_birthday { get; set; }
        public Nullable<int> candidate_domicile_id { get; set; }
        public Nullable<int> candidate_gender { get; set; }
        public string candidate_place { get; set; }
        public string candidate_domicile { get; set; }
        public string candidate_identity { get; set; }
        public Nullable<int> resident_curent_address_id { get; set; }
        public Nullable<int> resident_address_id { get; set; }
        public Nullable<System.DateTime> candidate_identity_date { get; set; }
        public Nullable<int> candidate_identity_place { get; set; }
        public Nullable<int> candidate_place_id { get; set; }
        public Nullable<int> candidate_marital { get; set; }
        public Nullable<int> candidate_nationality { get; set; }
        public Nullable<int> candidate_ethnic { get; set; }
        public Nullable<int> candidate_religion { get; set; }
        public Nullable<int> candidate_height { get; set; }
        public Nullable<int> candidate_weight { get; set; }
        public Nullable<int> candidate_military { get; set; }
        public string candidate_introduce { get; set; }
        public string candidate_phone { get; set; }
        public string candidate_email { get; set; }
        public string resident { get; set; }
        public string resident_address { get; set; }
        public string resident_current { get; set; }
        public string resident_curent_address { get; set; }
        public Nullable<int> organization_id { get; set; }
        public Nullable<int> is_order { get; set; }
        public Nullable<int> status { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_token_id { get; set; }
        public string modified_ip { get; set; }
    }
}