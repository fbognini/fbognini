using System;

namespace fbognini.Core.Domain;

public interface IHaveCreated
{
    public DateTime CreatedOnUtc { get; set; }
}
