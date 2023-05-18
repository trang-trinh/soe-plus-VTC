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
    
    public partial class hrm_insurance_pay
    {
        public string insurance_pay_id { get; set; }
        public string insurance_id { get; set; }
        public string profile_id { get; set; }
        public Nullable<System.DateTime> start_date { get; set; }
        public Nullable<System.DateTime> end_date { get; set; }
        public string payment_form { get; set; }
        public string reason { get; set; }
        public string organization_payment { get; set; }
        public Nullable<int> total_payment { get; set; }
        public Nullable<int> company_payment { get; set; }
        public Nullable<int> member_payment { get; set; }
        public Nullable<int> is_order { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string organization_name { get; set; }
        public string title_name { get; set; }
        public Nullable<double> coef_salary { get; set; }
        public Nullable<double> coef_allowance { get; set; }
    
        public virtual hrm_insurance hrm_insurance { get; set; }
    }
}