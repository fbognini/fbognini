﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Core.Domain.Query;

public class PageAuditableCriteria: PageCriteria
{
    public long? Since { get; internal set; }
    public long? AfterId { get; internal set; }
    internal string? ContinuationSince { get; set; }
}
