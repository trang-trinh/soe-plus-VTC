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
    
    public partial class backup_history
    {
        public string backup_history_id { get; set; }
        public string backup_id { get; set; }
        public string title_backup { get; set; }
        public Nullable<System.DateTime> backup_date { get; set; }
        public Nullable<int> status { get; set; }
        public string file_backup_path { get; set; }
        public Nullable<double> file_size { get; set; }
        public string created_by { get; set; }
    }
}
