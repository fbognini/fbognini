using System;

namespace fbognini.Core.Domain;

public interface IHaveLastUpdated
{
    public DateTime LastUpdatedOnUtc { get; set; }
}
