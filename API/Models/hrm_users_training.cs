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
    
    public partial class hrm_users_training
    {
        public int users_training_id { get; set; }
        public int training_emps_id { get; set; }
        public string profile_user_name { get; set; }
        public Nullable<int> department_id { get; set; }
        public Nullable<int> work_position_id { get; set; }
        public Nullable<int> position_id { get; set; }
        public string note { get; set; }
        public string profile_id { get; set; }
        public Nullable<int> organization_id { get; set; }
        public Nullable<int> is_order { get; set; }
        public Nullable<bool> status { get; set; }
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