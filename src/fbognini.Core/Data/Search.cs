using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Core.Data
{
    public class Search<TEntity>
    {
        public List<Expression<Func<TEntity, object>>> Fields { get; } = new();
        public List<string> FieldStrings { get; set; } = new();
        public string Keyword { get; set; }
    }
}
