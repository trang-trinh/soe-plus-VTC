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
    
    public partial class hrm_training_emps
    {
        public int training_emps_id { get; set; }
        public Nullable<int> training_groups_id { get; set; }
        public string training_emps_code { get; set; }
        public string training_emps_name { get; set; }
        public Nullable<System.DateTime> start_date { get; set; }
        public Nullable<System.DateTime> end_date { get; set; }
        public Nullable<int> form_training { get; set; }
        public Nullable<int> obj_training { get; set; }
        public string training_place { get; set; }
        public Nullable<int> organization_training { get; set; }
        public string user_verify { get; set; }
        public string user_follows { get; set; }
        public Nullable<int> training_times { get; set; }
        public Nullable<System.DateTime> registration_deadline { get; set; }
        public Nullable<int> organization_id { get; set; }
        public Nullable<int> is_order { get; set; }
        public Nullable<double> expense { get; set; }
        public Nullable<double> tuition { get; set; }
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