using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TraceabilityV3.Models
{
    public class APIzTraceOut_SSCCUnit
    {       
        public string WERKS { get; set; }
        public string MATNR { get; set; }
        public string SSCC { get; set; }
        public int WaypointId { get; set; }
        public string Waypoint { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public TimeSpan? CreatedTime { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public decimal ActualWeight { get; set; }
        public string ScannedGTIN { get; set; }
        public string Label_GTIN { get; set; }
        public string Label_Description { get; set; }
        public string Label_Country { get; set; }
        public string PData_DateCode { get; set; }
        public string PData_DateTime { get; set; }
        public string PData_BatchCode { get; set; }
        public string PData_BestBefore { get; set; }
        public string PDet_PCode { get; set; }
        public int PDet_OrdUnits { get; set; }
        public int PDet_ConsUnits { get; set; }
        public decimal PDet_NetWt { get; set; }
        public string VoidedX { get; set; }
        public string VoidedReason { get; set; }
        public string AcceptedX { get; set; }
    }
}