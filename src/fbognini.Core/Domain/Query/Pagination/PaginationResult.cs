namespace fbognini.Core.Domain.Query.Pagination;

public class PaginationResult
{

    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public int? Total { get; set; }
    public bool? AtLeast { get; set; }
    public string? ContinuationSince { get; set; }
    public int? PartialTotal { get; set; }
}
