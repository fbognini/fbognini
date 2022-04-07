using FluentValidation;

namespace fbognini.Core.Data.Pagination
{
    public abstract class PaginationQuery
    {
        /// <summary>
        /// [Pagination] - number of elements to be returned
        /// </summary>
        /// <example>5</example>
        public int? PageSize { get; set; }
    }


    public class PaginationQueryValidator : AbstractValidator<PaginationQuery>
    {
        public PaginationQueryValidator()
        {
            RuleFor(x => x.PageSize).GreaterThan(1).When(x => x.PageSize.HasValue);
        }
    }
}
