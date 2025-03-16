using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TraceabilityV3.Models
{
    public class APIHandlingUnitMovement
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
        public string ChildServer { get; set; }

    }
}