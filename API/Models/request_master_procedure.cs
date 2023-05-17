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
    
    public partial class request_master_procedure
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public request_master_procedure()
        {
            this.request_master_sign = new HashSet<request_master_sign>();
        }
    
        public string procedure_id { get; set; }
        public string request_id { get; set; }
        public string procedure_name { get; set; }
        public Nullable<int> is_type { get; set; }
        public Nullable<bool> status { get; set; }
        public Nullable<int> is_order { get; set; }
        public Nullable<bool> is_close { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_ip { get; set; }
        public string modified_token_id { get; set; }
        public Nullable<int> organization_id { get; set; }
    
        public virtual request_master request_master { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<request_master_sign> request_master_sign { get; set; }
    }
}
