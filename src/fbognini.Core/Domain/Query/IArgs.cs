using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Core.Domain.Query;

public interface IArgs
{
    string GetArgsKey();
    Dictionary<string, object?> GetArgsKeyAsDictionary();
}
