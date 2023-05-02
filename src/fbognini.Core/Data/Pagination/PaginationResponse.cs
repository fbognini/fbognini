using System.Collections.Generic;

namespace fbognini.Core.Data.Pagination
{
    public class PaginationResponse<TClass>
        where TClass : class
    {
        public PaginationResponse()
        {
            Pagination = new PaginationResult();
        }

        public List<TClass> Items { get; set; }
        public PaginationResult Pagination { get; set; }
    }
}
