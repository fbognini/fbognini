using System.Collections.Generic;

namespace fbognini.Infrastructure.Multitenancy
{
    public class MultitenancySettings
    {
        public bool IncludeAll { get; set; }
        public List<string> IncludePaths { get; set; }
        public List<string> ExcludePaths { get; set; }
    }
}
