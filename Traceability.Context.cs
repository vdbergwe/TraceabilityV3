﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TraceabilityEntities : DbContext
    {
        public TraceabilityEntities()
            : base("name=TraceabilityEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Device> Devices { get; set; }
        public virtual DbSet<GUSHandlingUnit> GUSHandlingUnits { get; set; }
        public virtual DbSet<HandlingUnitMovement> HandlingUnitMovements { get; set; }
        public virtual DbSet<HandlingUnit> HandlingUnits { get; set; }
        public virtual DbSet<Operator> Operators { get; set; }
        public virtual DbSet<Plant> Plants { get; set; }
        public virtual DbSet<RejectReason> RejectReasons { get; set; }
        public virtual DbSet<SAPMaterial> SAPMaterials { get; set; }
        public virtual DbSet<SSCCNumberBank> SSCCNumberBanks { get; set; }
        public virtual DbSet<Waypoint> Waypoints { get; set; }
        public virtual DbSet<IssuedLabel> IssuedLabels { get; set; }
    }
}
