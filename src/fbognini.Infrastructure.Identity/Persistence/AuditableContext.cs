using fbognini.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using fbognini.Application.Entities;
using fbognini.Core.Entities;
using fbognini.Infrastructure.Persistence;
using Finbuckle.MultiTenant;
using fbognini.Infrastructure.Utilities;
using fbognini.Infrastructure.Models;

namespace fbognini.Infrastructure.Identity.Persistence
{
    public class AuditableContext<TContext, TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey>, IBaseDbContext
        where TContext : DbContext
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly ICurrentUserService currentUserService;
        private readonly ITenantInfo currentTenant;

        private readonly string authschema;

        public AuditableContext(
            DbContextOptions<TContext> options,
            ICurrentUserService currentUserService,
            ITenantInfo currentTenant,
            string authschema = "auth")
            : base(options)
        {
            this.currentUserService = currentUserService;
            this.currentTenant = currentTenant;
            this.authschema = authschema;
        }

        public DbSet<Audit> AuditTrails { get; set; }
        public string UserId => currentUserService.UserId;
        public string Tenant => currentTenant.Name;
        public string ConnectionString => currentTenant?.ConnectionString;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.OnCustomConfiguring(this);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.OnCustomModelCreating(this);
            builder.ApplyIdentityConfiguration<TUser, TRole, TKey>(authschema);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return await this.AuditableSaveChangesAsync(() => base.SaveChangesAsync(cancellationToken), cancellationToken);
        }
    }
}
