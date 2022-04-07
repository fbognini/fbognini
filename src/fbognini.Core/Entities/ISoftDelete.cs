using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Core.Entities
{
    public interface ISoftDelete
    {
        DateTime? Deleted { get; set; }
        string DeletedBy { get; set; }
    }
}
