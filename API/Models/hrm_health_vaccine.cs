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
    
    public partial class hrm_health_vaccine
    {
        public int vaccine_id { get; set; }
        public string health_id { get; set; }
        public string profile_id { get; set; }
        public Nullable<int> injection_id { get; set; }
        public Nullable<System.DateTime> injection_date { get; set; }
        public string type_vaccine { get; set; }
        public string lot_number { get; set; }
        public string vaccination_facility { get; set; }
        public string sign_user { get; set; }
        public string sign_user_position { get; set; }
        public Nullable<int> is_order { get; set; }
    
        public virtual hrm_profile_health hrm_profile_health { get; set; }
    }
}