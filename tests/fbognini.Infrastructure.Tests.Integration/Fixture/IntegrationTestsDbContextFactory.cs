using fbognini.Core.Interfaces;
using fbognini.Infrastructure.Outbox;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;

namespace fbognini.Infrastructure.Tests.Integration.Fixture
{
    internal class IntegrationTestsDbContextFactory : IDbContextFactory<IntegrationTestsDbContext>
    {
        private readonly DbContextOptions<IntegrationTestsDbContext> options;
        private readonly ICurrentUserService currentUserService;
        private readonly IOutboxMessagesListener outboxMessagesListener;
        private readonly ITenantInfo currentTenant;

        public IntegrationTestsDbContextFactory(DbContextOptions<IntegrationTestsDbContext> options, ICurrentUserService currentUserService, IOutboxMessagesListener outboxMessagesListener, ITenantInfo currentTenant)
        {
            this.options = options;
            this.currentUserService = currentUserService;
            this.outboxMessagesListener = outboxMessagesListener;
            this.currentTenant = currentTenant;
        }

        public IntegrationTestsDbContext CreateDbContext()
        {
            return new IntegrationTestsDbContext(options, currentUserService, outboxMessagesListener, currentTenant);
        }
    }
}
