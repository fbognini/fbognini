using FluentValidation;

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
    public class PaginationOffsetQueryValidator : AbstractValidator<PaginationOffsetQuery>
    {
        public PaginationOffsetQueryValidator()
        {
            Include(new PaginationQueryValidator());
            RuleFor(x => x.PageNumber).GreaterThan(0).When(x => x.PageNumber.HasValue);
        }
    }
}
