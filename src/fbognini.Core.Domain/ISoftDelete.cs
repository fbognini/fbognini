using System;

namespace fbognini.Core.Domain;

public interface ISoftDelete
{
    DateTime? DeletedOnUtc { get; set; }
    string? DeletedBy { get; set; }
}
