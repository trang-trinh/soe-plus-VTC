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
    
    public partial class api_commentbug
    {
        public int comment_id { get; set; }
        public int bug_id { get; set; }
        public string des { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string url_file { get; set; }
        public string user_id { get; set; }
        public Nullable<int> parent_id { get; set; }
        public Nullable<int> bugcomment_id { get; set; }
        public Nullable<bool> is_add { get; set; }
    
        public virtual api_bug api_bug { get; set; }
    }
}
