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
    
    public partial class request_follow
    {
        public string request_follow_id { get; set; }
        public string request_id { get; set; }
        public string user_id { get; set; }
        public Nullable<bool> status_follow { get; set; }
        public Nullable<int> is_type { get; set; }
        public Nullable<int> is_order { get; set; }
        public Nullable<bool> is_app { get; set; }
        public Nullable<bool> is_notify { get; set; }
        public Nullable<bool> is_mail { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_ip { get; set; }
        public string modified_token_id { get; set; }
    }
}
