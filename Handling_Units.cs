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
    
    public partial class Handling_Units
    {
        public int Id { get; set; }
        public string SSCC { get; set; }
        public Nullable<int> Product { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public string CreatedBy { get; set; }
        public string Status { get; set; }
        public Nullable<int> NumberBank { get; set; }
        public string Device { get; set; }
        public Nullable<int> Batch { get; set; }
        public string ScannedCode { get; set; }
        public string Horse { get; set; }
        public string Trailer { get; set; }
        public string Berth { get; set; }
        public string Description { get; set; }
    
        public virtual Production_Batches Production_Batches { get; set; }
        public virtual Product Product1 { get; set; }
    }
}
