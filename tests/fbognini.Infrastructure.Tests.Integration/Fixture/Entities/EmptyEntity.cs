using fbognini.Core.Domain;

namespace fbognini.Infrastructure.Tests.Integration.Fixture.Entities
{
    public class EmptyEntity: IHasIdentity<int>
    {
        public int Id { get; set; }
    }
}
