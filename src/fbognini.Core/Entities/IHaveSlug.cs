using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Core.Entities
{
    public interface IHaveSlug
    {
        string Slug { get; set; }
    }
}
