namespace fbognini.Core.Data.Pagination
{
    public class PaginationOffsetQuery : PaginationQuery
    {
        public PaginationOffsetQuery() { }

        public PaginationOffsetQuery(
            int pageSize
            , int pageNumber)
        {
            PageSize = pageSize;
            PageNumber = pageNumber;
        }

        /// <summary>
        /// [Pagination] - number of page to be returned - offset = (PageNumber - 1) * PageSize
        /// </summary>
        /// <example>1</example>
        public int? PageNumber { get; set; }
    }
}
