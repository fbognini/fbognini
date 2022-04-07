using System.Collections.Generic;

namespace fbognini.Core.Interfaces
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        string UserName { get; }
        bool HasClaim(string type, string value);
        List<string> GetRoles();
    }
}
