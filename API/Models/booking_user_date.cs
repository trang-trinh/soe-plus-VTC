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
    
    public partial class booking_user_date
    {
        public int booking_date_id { get; set; }
        public Nullable<System.DateTime> booking_date { get; set; }
        public string user_id { get; set; }
        public Nullable<int> is_order { get; set; }
        public string booking_id { get; set; }
    
        public virtual booking_meal booking_meal { get; set; }
    }
}
