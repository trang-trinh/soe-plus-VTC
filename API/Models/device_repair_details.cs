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
    
    public partial class device_repair_details
    {
        public int repair_details_id { get; set; }
        public int device_repair_id { get; set; }
        public int card_id { get; set; }
        public string device_name { get; set; }
        public string serial { get; set; }
        public Nullable<int> device_id { get; set; }
        public Nullable<System.DateTime> purchase_date { get; set; }
        public Nullable<double> price { get; set; }
        public Nullable<double> current_price { get; set; }
        public string repair_plan { get; set; }
        public string condition { get; set; }
        public string repair_note { get; set; }
        public Nullable<double> repair_price { get; set; }
        public Nullable<int> repair_condition { get; set; }
        public Nullable<System.DateTime> repair_date { get; set; }
        public string assess_repair_condition { get; set; }
        public Nullable<int> is_order { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_token_id { get; set; }
        public string modified_ip { get; set; }
        public Nullable<int> is_replace { get; set; }
    }
}