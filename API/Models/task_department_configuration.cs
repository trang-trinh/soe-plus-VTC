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
    
    public partial class task_department_configuration
    {
        public int id { get; set; }
        public Nullable<int> department_id { get; set; }
        public string user_id { get; set; }
    
        public virtual sys_organization sys_organization { get; set; }
    }
}