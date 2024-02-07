using fbognini.Core.Interfaces;

namespace SberemPay.Checkout.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService()
        {
        }

        public string UserId => Guid.NewGuid().ToString();

        public string UserName => Guid.NewGuid().ToString();

        public List<string> GetRoles()
        {
            throw new NotImplementedException();
        }

        public bool HasClaim(string type, string value)
        {
            throw new NotImplementedException();
        }
    }
}
