using System.Collections.Generic;

namespace fbognini.Core.Data.Pagination
{
    public class PaginationResponse<TClass>
        where TClass : class
    {
        public PaginationResponse()
        {
        }

        public List<TClass> Items { get; set; } = new List<TClass>();
        public PaginationResult? Pagination { get; set; }
    }
}
