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
    
    public partial class OMD_DatawarehouseEntities : DbContext
    {
        public OMD_DatawarehouseEntities()
            : base("name=OMD_DatawarehouseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Handling_Units> Handling_Units { get; set; }
        public virtual DbSet<Production_Batches> Production_Batches { get; set; }
        public virtual DbSet<Product> Products { get; set; }
    }
}
