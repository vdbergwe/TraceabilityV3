using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TraceabilityV3.Models
{
    public class HandlingUnitsViewModel
    {
        public int ProductId { get; set; }
        public string PLU { get; set; }
        public string Description { get; set; }
        public int TotalScanned { get; set; }
        public int TotalRejected { get; set; }
        public int TotalAccepted { get; set; }
        public int TotalPending { get; set; }
        public decimal TotalKG { get; set; }
        public List<HandlingUnitsItemViewModel> ProductItems { get; set; }
    }

    public class HandlingUnitsItemViewModel
    {
        public string SSCC { get; set; }
        public DateTime? Created { get; set; }
        public string BatchDescription { get; set; }
        public string RejectReason { get; set; }
    }

}