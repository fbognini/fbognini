﻿using fbognini.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using fbognini.Infrastructure.Entities;
using fbognini.Infrastructure.Outbox;
using fbognini.Infrastructure.Persistence;

namespace fbognini.Infrastructure.Persistence
{
    public class IdentityAuditableDbContext<TContext, TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey>, IBaseDbContext
        where TContext : DbContext
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly ICurrentUserService currentUserService;
        private readonly IOutboxMessagesListener outboxListenerService;
        private readonly ITenantInfo? currentTenant;

        protected readonly string authschema;

        public IdentityAuditableDbContext(
            DbContextOptions<TContext> options,
            ICurrentUserService currentUserService,
            IOutboxMessagesListener outboxListenerService,
            ITenantInfo? currentTenant = null,
            string authschema = "auth")
            : base(options)
        {
            this.currentUserService = currentUserService;
            this.outboxListenerService = outboxListenerService;
            this.currentTenant = currentTenant;
            this.authschema = authschema;
        }

        public DbSet<Audit> AuditTrails { get; set; } = default!;
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = default!;

        public string? UserId => currentUserService.UserId;
        public DateTime Timestamp => DateTime.Now;
        public string? Tenant => currentTenant?.Id;
        public string? ConnectionString => currentTenant?.ConnectionString;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureSqlServer(this);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsAndFilters(this);
            builder.ApplyIdentityConfiguration<TUser, TRole, TKey>(authschema);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return this.AuditableSaveChangesAsync(outboxListenerService, cancellationToken);
        }

        public Task<int> BaseSaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
