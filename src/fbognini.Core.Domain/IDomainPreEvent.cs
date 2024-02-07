using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Core.Domain;

public interface IDomainPreEvent
{
    IDomainEvent ToDomainEvent();
}
