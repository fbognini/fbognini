using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using fbognini.Core.Interfaces;
using Finbuckle.MultiTenant;
using fbognini.Infrastructure.Entities;
using System;
using fbognini.Infrastructure.Extensions;

namespace fbognini.Infrastructure.Persistence
{
    public class AuditableDbContext<T> : DbContext, IBaseDbContext
        where T : DbContext
    {
        private readonly ICurrentUserService currentUserService;
        private readonly ITenantInfo currentTenant;

        public AuditableDbContext(
            DbContextOptions<T> options,
            ICurrentUserService currentUserService,
            ITenantInfo currentTenant)
            : base(options)
        {
            this.currentUserService = currentUserService;
            this.currentTenant = currentTenant;
        }

        public DbSet<Audit> AuditTrails { get; set; }
        public string UserId => currentUserService.UserId;
        public DateTime Timestamp => DateTime.Now;
        public string Tenant => currentTenant.Id;
        public string? ConnectionString => currentTenant?.ConnectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureSqlServer(this);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsAndFilters(this);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return this.AuditableSaveChangesAsync(cancellationToken);
        }

        public Task<int> BaseSaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
