using fbognini.Core.Data;
using WebApplication1.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Application.SearchCriterias
{
    public class AuthorSearchCriteria: SearchCriteria<Author>
    {

        public override List<Expression<Func<Author, bool>>> ToWhereClause()
        {
            var list = new List<Expression<Func<Author, bool>>>();

            return list;
        }
    }
}
