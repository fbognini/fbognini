using WebApplication1.Domain.Entities;
using System.Linq.Expressions;
using fbognini.Core.Domain.Query;

namespace WebApplication1.SearchCriterias
{
    public class BookSearchCriteria : SelectCriteria<Book>
    {
        public string? Title { get; set; }

        public override List<Expression<Func<Book, bool>>> ToWhereClause()
        {
            var list = new List<Expression<Func<Book, bool>>>();

            if (!string.IsNullOrEmpty(Title))
            {
                list.Add(x => x.Title == Title);
            }

            return list;
        }
    }
}
