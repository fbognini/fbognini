using Microsoft.EntityFrameworkCore;

namespace fbognini.Infrastructure.Persistence
{
    public class DatabaseSettings
    {
        public string DBProvider { get; set; } = string.Empty;
        public string? ConnectionString { get; set; }
        public QueryTrackingBehavior TrackingBehavior { get; set; }
        public string AuthSchema { get; set; } = "auth";
    }
}
