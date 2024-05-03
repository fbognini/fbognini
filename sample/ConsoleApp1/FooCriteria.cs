using fbognini.Core.Domain.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Foo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class FooCriteria: QueryableCriteria<Foo>
    {
        public string? Title { get; set; }
        public string? Urca { get; set; }
    }
}
