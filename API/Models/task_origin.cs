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
    
    public partial class task_origin
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public task_origin()
        {
            this.task_extend = new HashSet<task_extend>();
            this.task_follow = new HashSet<task_follow>();
            this.task_linkdoc = new HashSet<task_linkdoc>();
        }
    
        public string task_id { get; set; }
        public string task_code { get; set; }
        public string parent_id { get; set; }
        public string project_id { get; set; }
        public string checklist_id { get; set; }
        public Nullable<bool> is_check { get; set; }
        public Nullable<int> department_id { get; set; }
        public Nullable<int> group_id { get; set; }
        public string task_name { get; set; }
        public string task_name_en { get; set; }
        public string description { get; set; }
        public string keywords { get; set; }
        public Nullable<System.DateTime> start_date { get; set; }
        public Nullable<System.DateTime> end_date { get; set; }
        public Nullable<System.DateTime> start_real_date { get; set; }
        public Nullable<System.DateTime> end_real_date { get; set; }
        public Nullable<int> weight { get; set; }
        public string difficult { get; set; }
        public string target { get; set; }
        public string result { get; set; }
        public string request { get; set; }
        public Nullable<bool> is_prioritize { get; set; }
        public Nullable<bool> is_deadline { get; set; }
        public Nullable<bool> is_review { get; set; }
        public Nullable<double> progress { get; set; }
        public Nullable<bool> is_todo { get; set; }
        public Nullable<bool> is_public { get; set; }
        public Nullable<bool> is_security { get; set; }
        public string close_by { get; set; }
        public Nullable<System.DateTime> close_date { get; set; }
        public Nullable<System.DateTime> finish_date { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> is_order { get; set; }
        public string created_by { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public string created_ip { get; set; }
        public string created_token_id { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_date { get; set; }
        public string modified_ip { get; set; }
        public string modified_token_id { get; set; }
        public Nullable<int> u_department_id { get; set; }
        public Nullable<int> organization_id { get; set; }
        public Nullable<int> organization_child_id { get; set; }
        public Nullable<bool> is_template { get; set; }
        public Nullable<int> process_time { get; set; }
    
        public virtual task_ca_taskgroup task_ca_taskgroup { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<task_extend> task_extend { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<task_follow> task_follow { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<task_linkdoc> task_linkdoc { get; set; }
    }
}