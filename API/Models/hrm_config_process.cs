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
    
    public partial class hrm_config_process
    {
        public int config_process_id { get; set; }
        public Nullable<int> type_module { get; set; }
        public string config_process_name { get; set; }
        public Nullable<int> key_id { get; set; }
        public string user_id { get; set; }
        public Nullable<bool> is_approved { get; set; }
        public Nullable<int> users_form_id { get; set; }
        public Nullable<int> aproved_groups_id { get; set; }
        public Nullable<int> process_form_id { get; set; }
        public Nullable<bool> is_last { get; set; }
        public Nullable<int> pre_process_id { get; set; }
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
