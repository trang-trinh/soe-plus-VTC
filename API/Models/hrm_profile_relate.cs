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
    
    public partial class hrm_profile_relate
    {
        public int key_id { get; set; }
        public string profile_id { get; set; }
        public string relate_id { get; set; }
        public Nullable<System.DateTime> relate_time { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
    
        public virtual hrm_profile hrm_profile { get; set; }
    }
}
