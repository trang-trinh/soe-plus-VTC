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
    
    public partial class doc_codes
    {
        public int code_id { get; set; }
        public Nullable<int> organization_id { get; set; }
        public string code_master_id { get; set; }
        public string idkey { get; set; }
        public Nullable<int> is_order { get; set; }
        public string info_col { get; set; }
        public Nullable<bool> is_used { get; set; }
        public string separator { get; set; }
        public Nullable<bool> auto_gen { get; set; }
        public Nullable<int> nav_type { get; set; }
        public Nullable<bool> num_by_group { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_token_id { get; set; }
        public string modified_ip { get; set; }
    
        public virtual doc_codes_master doc_codes_master { get; set; }
        public virtual sys_organization sys_organization { get; set; }
    }
}
