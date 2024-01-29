using Microsoft.EntityFrameworkCore;

namespace fbognini.Infrastructure.Multitenancy
{
    public class DatabaseSettings
    {
        public string? DBProvider { get; set; }
        public string? ConnectionString { get; set; }
        public QueryTrackingBehavior TrackingBehavior { get; set; }
    }
}
