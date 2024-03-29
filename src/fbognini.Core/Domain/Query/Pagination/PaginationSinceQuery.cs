﻿namespace fbognini.Core.Domain.Query.Pagination;

public class PaginationAdvancedSinceQuery : PaginationQuery
{
    public PaginationAdvancedSinceQuery()
           : this(100)
    {
    }

    public PaginationAdvancedSinceQuery(int pageSize, string? since = null)
    {
        PageSize = pageSize;
        Since = since;
    }

    /// <summary>
    /// [Pagination] - milliseconds from epoch
    /// </summary>
    /// <example>1588725310636_id</example>
    public string? Since { get; set; }
}


public class PaginationSinceQuery : PaginationQuery
{
    public PaginationSinceQuery()
           : this(100)
    {
    }

    public PaginationSinceQuery(int pageSize, long? since = null)
    {
        PageSize = pageSize;
        Since = since;
    }

    /// <summary>
    /// [Pagination] - milliseconds from epoch
    /// </summary>
    /// <example>1588725310636</example>
    public long? Since { get; set; }
}
