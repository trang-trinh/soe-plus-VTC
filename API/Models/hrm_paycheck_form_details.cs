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
    
    public partial class hrm_paycheck_form_details
    {
        public int paycheck_form_details_id { get; set; }
        public int paycheck_form_id { get; set; }
        public string paycheck_name { get; set; }
        public Nullable<int> paycheck_type { get; set; }
        public string paycheck_code { get; set; }
        public string paycheck_unit { get; set; }
        public Nullable<int> organization_id { get; set; }
        public Nullable<int> paycheck_id { get; set; }
        public Nullable<int> declare_id { get; set; }
        public Nullable<int> parent_id { get; set; }
        public Nullable<int> paycheck_input { get; set; }
        public Nullable<int> is_order { get; set; }
        public string type_order { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_token_id { get; set; }
        public string modified_ip { get; set; }
    
        public virtual hrm_paycheck_form hrm_paycheck_form { get; set; }
    }
}