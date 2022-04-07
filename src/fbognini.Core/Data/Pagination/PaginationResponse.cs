using System.Collections.Generic;

namespace fbognini.Core.Data.Pagination
{
    public class PaginationResponse<TClass>
        where TClass : class
    {
        public PaginationResponse()
        {
            Pagination = new Pagination();
        }
        public List<TClass> Response { get; set; }
        public Pagination Pagination { get; set; }
    }
}
