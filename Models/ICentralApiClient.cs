using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace TraceabilityV3.Models
{
    public interface ICentralApiClient    
    {
        Task UploadRecord(HandlingUnit record);
        Task UploadMovementRecord(HandlingUnitMovement record);
    }
}