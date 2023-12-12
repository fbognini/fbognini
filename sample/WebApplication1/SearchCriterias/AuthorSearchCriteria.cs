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
    public class AuthorSearchCriteria : SearchCriteria<Author>
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
