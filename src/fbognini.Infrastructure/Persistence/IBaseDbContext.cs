using fbognini.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace fbognini.Infrastructure.Persistence
{
    public interface IBaseDbContext
    {
        DbSet<Audit> AuditTrails { get; set; }
        public string UserId { get; }
        public string Tenant { get; }
        public string ConnectionString { get; }
    }
}
