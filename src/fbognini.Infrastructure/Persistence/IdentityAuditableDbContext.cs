using fbognini.Core.Interfaces;
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
using Microsoft.Extensions.Options;
using fbognini.Infrastructure.Common;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;

namespace fbognini.Infrastructure.Persistence
{
    public class IdentityAuditableDbContext<TContext, TUser, TRole, TKey> : IdentityDbContext<TUser, TRole, TKey>, IBaseDbContext
        where TContext : DbContext
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IOutboxMessagesListener _outboxListenerService;
        private readonly ITenantInfo? _currentTenant;

        private readonly DatabaseSettings _databaseSettings;

        public IdentityAuditableDbContext(
            DbContextOptions<TContext> options,
            IOptions<DatabaseSettings> databaseOptions,
            ICurrentUserService currentUserService,
            IDateTimeProvider dateTimeProvider,
            IOutboxMessagesListener outboxListenerService,
            ITenantInfo? currentTenant = null)
            : base(options)
        {
            _databaseSettings = databaseOptions.Value;
            _currentUserService = currentUserService;
            _dateTimeProvider = dateTimeProvider;
            _outboxListenerService = outboxListenerService;
            _currentTenant = currentTenant;
        }

        public DbSet<Audit> AuditTrails { get; set; } = default!;
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = default!;

        public string? UserId => _currentUserService.UserId;
        public DateTime Timestamp => _dateTimeProvider.UtcNow;
        public string DBProvider => _databaseSettings.DBProvider;
        public string? Tenant => _currentTenant?.Id;
        public string? ConnectionString => _currentTenant?.ConnectionString;


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureDbProvider(this);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsAndFilters(this);
            builder.ApplyIdentityConfiguration<TUser, TRole, TKey>(_databaseSettings.AuthSchema!);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return this.AuditableSaveChangesAsync(_outboxListenerService, cancellationToken);
        }

        public Task<int> BaseSaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
