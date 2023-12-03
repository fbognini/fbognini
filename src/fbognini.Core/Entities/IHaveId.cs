using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Text;

namespace fbognini.Core.Entities
{
    public interface IHaveId<TKey>
        where TKey : notnull
    {
        public TKey Id { get; set; }
    }
}
