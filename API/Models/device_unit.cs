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
    
    public partial class device_unit
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public device_unit()
        {
            this.device_main = new HashSet<device_main>();
        }
    
        public int device_unit_id { get; set; }
        public string device_unit_name { get; set; }
        public int organization_id { get; set; }
        public Nullable<bool> status { get; set; }
        public Nullable<int> is_order { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_token_id { get; set; }
        public string modified_ip { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<device_main> device_main { get; set; }
    }
}
