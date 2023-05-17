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
    
    public partial class chat_file
    {
        public string file_id { get; set; }
        public string chat_group_id { get; set; }
        public string chat_message_id { get; set; }
        public string file_type { get; set; }
        public string file_name { get; set; }
        public Nullable<double> file_size { get; set; }
        public string file_path { get; set; }
        public Nullable<bool> is_image { get; set; }
        public Nullable<bool> is_delete { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string deleted_by { get; set; }
        public Nullable<System.DateTime> deleted_date { get; set; }
        public string deleted_ip { get; set; }
        public string deleted_token_id { get; set; }
    
        public virtual chat_message chat_message { get; set; }
    }
}
