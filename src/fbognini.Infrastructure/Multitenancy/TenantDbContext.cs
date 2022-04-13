using fbognini.Application.Entities;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
using System;

namespace fbognini.Infrastructure.Multitenancy
{
    public class TenantDbContext : EFCoreStoreDbContext<Tenant>
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Tenant>().ToTable("Tenants", "multitenancy");
        }
    }
}
