using fbognini.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
