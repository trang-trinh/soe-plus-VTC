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
    
    public partial class task_main
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public task_main()
        {
            this.task_comment = new HashSet<task_comment>();
            this.task_test = new HashSet<task_test>();
        }
    
        public int task_id { get; set; }
        public Nullable<int> category_id { get; set; }
        public string task_name { get; set; }
        public string test_user_ids { get; set; }
        public string user_id { get; set; }
        public string des { get; set; }
        public string url_file { get; set; }
        public string keywords { get; set; }
        public Nullable<System.DateTime> estimated_date { get; set; }
        public Nullable<System.DateTime> actual_date { get; set; }
        public Nullable<double> estimated_hours { get; set; }
        public Nullable<double> actual_hours { get; set; }
        public Nullable<System.DateTime> end_date { get; set; }
        public int is_order { get; set; }
        public Nullable<int> parent_id { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<bool> is_plan { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public Nullable<int> is_important { get; set; }
        public Nullable<bool> is_view_test { get; set; }
        public Nullable<bool> is_view_work { get; set; }
        public Nullable<bool> is_outtime { get; set; }
        public string next_date { get; set; }
        public Nullable<bool> is_deadline { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_by { get; set; }
        public string partner_id { get; set; }
    
        public virtual task_category task_category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<task_comment> task_comment { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<task_test> task_test { get; set; }
    }
}