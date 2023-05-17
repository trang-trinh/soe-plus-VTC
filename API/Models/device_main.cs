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
    
    public partial class device_main
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public device_main()
        {
            this.device_card = new HashSet<device_card>();
        }
    
        public int device_id { get; set; }
        public string device_code { get; set; }
        public string device_name { get; set; }
        public Nullable<int> device_unit_id { get; set; }
        public Nullable<double> price { get; set; }
        public Nullable<int> depreciation_month { get; set; }
        public Nullable<int> device_type_id { get; set; }
        public Nullable<int> organization_id { get; set; }
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
        public Nullable<int> device_groups_id { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<device_card> device_card { get; set; }
        public virtual device_type device_type { get; set; }
        public virtual device_unit device_unit { get; set; }
        public virtual sys_organization sys_organization { get; set; }
    }
}
