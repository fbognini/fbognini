using fbognini.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Tests.Integration.Fixture.Entities
{
    public class EmptyEntity: IHasIdentity<int>
    {
        public int Id { get; set; }
    }
}
