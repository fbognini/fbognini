using fbognini.Core.Interfaces;
using Finbuckle.MultiTenant;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Tests.Integration.Fixture
{
    internal class IntegrationTestsDbContextFactory : IDbContextFactory<IntegrationTestsDbContext>
    {
        private readonly DbContextOptions<IntegrationTestsDbContext> options;
        private readonly ICurrentUserService currentUserService;
        private readonly ITenantInfo currentTenant;

        public IntegrationTestsDbContextFactory(DbContextOptions<IntegrationTestsDbContext> options, ICurrentUserService currentUserService, ITenantInfo currentTenant)
        {
            this.options = options;
            this.currentUserService = currentUserService;
            this.currentTenant = currentTenant;
        }

        public IntegrationTestsDbContext CreateDbContext()
        {
            return new IntegrationTestsDbContext(options, currentUserService, currentTenant);
        }
    }
}
