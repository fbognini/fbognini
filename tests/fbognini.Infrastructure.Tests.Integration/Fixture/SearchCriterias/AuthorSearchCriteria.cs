using fbognini.Core.Domain.Query;
using System.Linq.Expressions;

namespace fbognini.Infrastructure.Tests.Integration.Fixture.SearchCriterias
{
    internal class AuthorSearchCriteria: SelectCriteria<Author>
    {
        public string? LastName { get; set; }

        public override List<Expression<Func<Author, bool>>> ToWhereClause()
        {
            var list = new List<Expression<Func<Author, bool>>>();

            if (!string.IsNullOrWhiteSpace(LastName))
            {
                list.Add(x => x.LastName.Contains(LastName));
            }

            return list;
        }
    }


}
