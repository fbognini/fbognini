#nullable disable
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace fbognini.Persistence.CustomMigrationBuilder
{
    /// <summary>
    /// A <see cref="MigrationOperation"/> for dropping an existing user-defined table type
    /// </summary>
    public class DropUserDefinedTableTypeOperation : MigrationOperation
    {
        /// <summary>
        ///     The name of the user defined table type.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        ///     The schema that contains the user defined table type, or <c>null</c> if the default schema should be used.
        /// </summary>
        public virtual string Schema { get; set; }
    }

}
