using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace TraceabilityV3.Models
{
    public class TraceabilityDbContextFactory : IDbContextFactory<TraceabilityEntities>
    {
        public TraceabilityEntities Create()
        {
            return new TraceabilityEntities();
        }
    }
}