﻿namespace fbognini.Core.Data.Pagination
{
    public abstract class PaginationQuery
    {
        /// <summary>
        /// [Pagination] - number of elements to be returned
        /// </summary>
        /// <example>5</example>
        public int? PageSize { get; set; }
    }
}
