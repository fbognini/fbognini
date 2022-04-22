using System;

namespace fbognini.Core.Entities
{

    public interface IEntity
    {

    }

    public interface IHasIdentity<T> : IEntity
        where T : notnull
    {
        public T Id { get; set; }
    }

    public interface IAuditableEntity: IEntity
    {
        string CreatedBy { get; set; }

        DateTime Created { get; set; }

        string LastModifiedBy { get; set; }

        DateTime? LastModified { get; set; }

        string LastUpdatedBy { get; set; }

        DateTime LastUpdated { get; set; }
    }

    public abstract class AuditableEntity : IAuditableEntity
    {
        public string CreatedBy { get; set; }

        public DateTime Created { get; set; }

        public string LastModifiedBy { get; set; }

        public DateTime? LastModified { get; set; }

        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdated { get; set; }
    }

    public abstract class AuditableEntityWithIdentity<T> : AuditableEntity, IHasIdentity<T>
        where T : notnull
    {
        public T Id { get; set; }
    }

    public abstract class AuditableEntityWithIdentity : AuditableEntityWithIdentity<int>
    {

    }
}
