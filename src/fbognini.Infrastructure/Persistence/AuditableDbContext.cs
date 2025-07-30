using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using fbognini.Core.Interfaces;
using Finbuckle.MultiTenant;
using fbognini.Infrastructure.Entities;
using System;
using fbognini.Infrastructure.Outbox;
using Microsoft.Extensions.Options;
using fbognini.Infrastructure.Common;

namespace fbognini.Infrastructure.Persistence
{
    public class AuditableDbContext<T> : DbContext, IBaseDbContext
        where T : DbContext
    {
        protected readonly ICurrentUserService _currentUserService;
        protected readonly IDateTimeProvider _dateTimeProvider;
        protected readonly IOutboxMessagesListener _outboxListenerService;
        protected readonly ITenantInfo? _currentTenant;

        protected readonly DatabaseSettings _databaseSettings;

        public AuditableDbContext(
            DbContextOptions<T> options,
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
