using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Core.Domain.Query;

public class PageCriteria
{
    public int? Number { get; internal set; }
    public int? Size { get; internal set; }

    internal int? Total { get; set; }
}
