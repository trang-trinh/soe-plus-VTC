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
    
    public partial class hrm_profile_clan_history
    {
        public string profile_clan_history_id { get; set; }
        public string profile_id { get; set; }
        public string card_number { get; set; }
        public Nullable<int> form { get; set; }
        public string admission_place { get; set; }
        public string transfer_place { get; set; }
        public Nullable<System.DateTime> start_date { get; set; }
        public Nullable<System.DateTime> end_date { get; set; }
        public Nullable<int> is_order { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
    
        public virtual hrm_profile hrm_profile { get; set; }
    }
}