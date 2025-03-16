using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TraceabilityV3.Models
{
    public class APIHandlingUnit
    {
        public string SSCC { get; set; }
        public string WERKS { get; set; }
        public string MATNR { get; set; }
        public string ScannedCode { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public string CreatedBy { get; set; }
        public string Batch { get; set; }
        public string Horse { get; set; }
        public string Status { get; set; }
        public string ChildServer { get; set; }
    }   
}