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
    
    public partial class hrm_config_process_form
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public hrm_config_process_form()
        {
            this.hrm_config_approved_form = new HashSet<hrm_config_approved_form>();
        }
    
        public int config_process_form_id { get; set; }
        public string config_process_name { get; set; }
        public string module { get; set; }
        public Nullable<int> config_process_type { get; set; }
        public Nullable<bool> status { get; set; }
        public int is_order { get; set; }
        public Nullable<bool> is_close { get; set; }
        public Nullable<bool> is_approved { get; set; }
        public Nullable<bool> is_view { get; set; }
        public Nullable<System.DateTime> date_view { get; set; }
        public Nullable<int> organization_id { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_ip { get; set; }
        public string modified_token_id { get; set; }
        public Nullable<int> key_id { get; set; }
        public Nullable<int> type_module { get; set; }
        public Nullable<int> type_send { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hrm_config_approved_form> hrm_config_approved_form { get; set; }
    }
}