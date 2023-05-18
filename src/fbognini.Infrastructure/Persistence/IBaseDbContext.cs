using fbognini.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Persistence
{
    public interface IBaseDbContext
    {
        DbSet<Audit> AuditTrails { get; set; }
        public string UserId { get; }
        public DateTime Timestamp { get; }
        public string Tenant { get; }
        public string ConnectionString { get; }

        Task<int> BaseSaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
