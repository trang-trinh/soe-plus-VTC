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
    
    public partial class request_form_sign_user
    {
        public string request_form_sign_user_id { get; set; }
        public string request_form_sign_id { get; set; }
        public string user_id { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public Nullable<int> STT { get; set; }
        public Nullable<int> IsType { get; set; }
        public Nullable<double> IsSLA { get; set; }
        public Nullable<bool> status { get; set; }
    
        public virtual request_ca_form_sign request_ca_form_sign { get; set; }
    }
}
