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
    
    public partial class smart_proc
    {
        public int id { get; set; }
        public string proc_name { get; set; }
        public string proc_title { get; set; }
        public string proc_des { get; set; }
        public Nullable<int> is_order { get; set; }
        public Nullable<bool> is_proc { get; set; }
        public string database { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public Nullable<int> organization_id { get; set; }
    }
}