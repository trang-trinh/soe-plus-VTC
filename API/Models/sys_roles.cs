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
    
    public partial class sys_roles
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public sys_roles()
        {
            this.sys_role_modules = new HashSet<sys_role_modules>();
        }
    
        public string role_id { get; set; }
        public string role_name { get; set; }
        public Nullable<int> organization_child_id { get; set; }
        public Nullable<int> organization_id { get; set; }
        public int is_order { get; set; }
        public bool status { get; set; }
        public string text_color { get; set; }
        public string background_color { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_token_id { get; set; }
        public string modified_ip { get; set; }
        public Nullable<bool> is_system { get; set; }
        public Nullable<bool> is_organization { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<sys_role_modules> sys_role_modules { get; set; }
    }
}
