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
    
    public partial class doc_ca_role_group_department
    {
        public int role_group_department_id { get; set; }
        public Nullable<int> role_group_id { get; set; }
        public Nullable<int> department_id { get; set; }
        public string user_id { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_token_id { get; set; }
        public string modified_ip { get; set; }
        public Nullable<int> organization_id { get; set; }
    
        public virtual doc_ca_role_groups doc_ca_role_groups { get; set; }
    }
}
