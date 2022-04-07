using System;

namespace fbognini.Core.Entities
{
    public abstract class AuditableEntityWithIdentity<T> : AuditableEntity
        where T: notnull
    {
        public T Id { get; set; }
    }

    public abstract class AuditableEntityWithIdentity : AuditableEntityWithIdentity<int>
    {

    }

    public abstract class AuditableEntity: IAuditableEntity
    {
        public string CreatedBy { get; set; }

        public DateTime Created { get; set; }

        public string LastModifiedBy { get; set; }

        public DateTime? LastModified { get; set; }

        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdated { get; set; }
    }

    public interface IAuditableEntity
    {
        string CreatedBy { get; set; }

        DateTime Created { get; set; }

        string LastModifiedBy { get; set; }

        DateTime? LastModified { get; set; }

        string LastUpdatedBy { get; set; }

        DateTime LastUpdated { get; set; }
    }
}
