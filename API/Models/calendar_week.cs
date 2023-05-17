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
    
    public partial class calendar_week
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public calendar_week()
        {
            this.calendar_attend = new HashSet<calendar_attend>();
            this.calendar_coincide = new HashSet<calendar_coincide>();
            this.calendar_member = new HashSet<calendar_member>();
        }
    
        public string calendar_id { get; set; }
        public string parent_id { get; set; }
        public Nullable<System.DateTime> start_date { get; set; }
        public Nullable<System.DateTime> end_date { get; set; }
        public string boardroom_id { get; set; }
        public string place_name { get; set; }
        public string contents { get; set; }
        public string contents_en { get; set; }
        public string equip { get; set; }
        public string note { get; set; }
        public Nullable<int> is_type { get; set; }
        public Nullable<int> is_iterations { get; set; }
        public Nullable<int> distance_iterations { get; set; }
        public Nullable<int> numeric_iterations { get; set; }
        public Nullable<int> numeric_attendees { get; set; }
        public string invitee { get; set; }
        public Nullable<bool> is_important { get; set; }
        public Nullable<bool> is_private { get; set; }
        public Nullable<bool> is_meeting { get; set; }
        public Nullable<int> is_type_send { get; set; }
        public string car_id { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> is_order { get; set; }
        public Nullable<int> is_group { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_ip { get; set; }
        public string modified_token_id { get; set; }
        public Nullable<int> organization_id { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<calendar_attend> calendar_attend { get; set; }
        public virtual calendar_ca_boardroom calendar_ca_boardroom { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<calendar_coincide> calendar_coincide { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<calendar_member> calendar_member { get; set; }
    }
}
