using WebApplication1.Domain.Entities;
using System.Linq.Expressions;
using fbognini.Core.Domain.Query;

namespace WebApplication1.SearchCriterias
{
    public class AuthorSearchCriteria : SelectCriteria<Author>
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
