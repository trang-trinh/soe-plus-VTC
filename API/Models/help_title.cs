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
    
    public partial class help_title
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public help_title()
        {
            this.help_content = new HashSet<help_content>();
        }
    
        public int help_title_id { get; set; }
        public Nullable<int> parent_id { get; set; }
        public string title_name { get; set; }
        public Nullable<int> is_order { get; set; }
        public string image { get; set; }
        public string file_name { get; set; }
        public Nullable<double> file_size { get; set; }
        public Nullable<int> is_level { get; set; }
        public Nullable<bool> status { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<help_content> help_content { get; set; }
    }
}