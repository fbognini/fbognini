using fbognini.Core.Data;
using WebApplication1.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.SearchCriterias
{
    public class BookSearchCriteria : SearchCriteria<Book>
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
