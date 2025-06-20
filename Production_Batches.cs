//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TraceabilityV3
{
    using System;
    using System.Collections.Generic;
    
    public partial class Production_Batches
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Production_Batches()
        {
            this.Handling_Units = new HashSet<Handling_Units>();
        }
    
        public int Id { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public Nullable<int> Shift { get; set; }
        public Nullable<System.DateTime> BatchDate { get; set; }
        public Nullable<System.DateTime> BatchCreated { get; set; }
        public Nullable<System.DateTime> BatchEnded { get; set; }
        public string Plant { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Handling_Units> Handling_Units { get; set; }
    }
}
