using Microsoft.AspNetCore.Identity;
using System;

namespace fbognini.Core.Entities
{
    public abstract class AuditableUser<TKey> : IdentityUser<TKey>, IAuditableEntity where TKey : IEquatable<TKey>
    {
        public AuditableUser()
            : base()
        {

        }

        public string CreatedBy { get; set; }

        public DateTime Created { get; set; }

        public string LastModifiedBy { get; set; }

        public DateTime? LastModified { get; set; }

        public string LastUpdatedBy { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
