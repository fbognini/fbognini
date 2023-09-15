using fbognini.Core.Extensions;
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
        public List<string> AllFields
        {
            get
            {
                var allFields = Fields.Select(x => PropertyExtensions.GetPropertyPath(x, true)).ToList();
                allFields.AddRange(FieldStrings);

                return allFields;
            }
        }
        public List<Expression<Func<TEntity, object>>> Fields { get; } = new List<Expression<Func<TEntity, object>>>();
        public List<string> FieldStrings { get; set; } = new List<string>();
        public string? Keyword { get; set; }
    }
}
