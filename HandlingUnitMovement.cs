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
    
    public partial class HandlingUnitMovement
    {
        public int Id { get; set; }
        public string SSCC { get; set; }
        public string Device { get; set; }
        public Nullable<System.DateTime> MovementTime { get; set; }
        public string CreatedBy { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Horse { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Waypoint { get; set; }
        public string RejectID { get; set; }
        public Nullable<bool> IsUploaded { get; set; }
    }
}
