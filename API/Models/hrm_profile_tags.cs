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
    
    public partial class hrm_profile_tags
    {
        public int key_id { get; set; }
        public string profile_id { get; set; }
        public int tags_id { get; set; }
        public Nullable<int> is_order { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_token_id { get; set; }
        public string modified_ip { get; set; }
    
        public virtual hrm_ca_tags hrm_ca_tags { get; set; }
        public virtual hrm_profile hrm_profile { get; set; }
    }
}
