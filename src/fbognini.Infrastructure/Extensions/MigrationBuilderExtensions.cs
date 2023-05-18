using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace fbognini.Infrastructure.Extensions
{
    public static class MigrationBuilderExtensions
    {
        public static OperationBuilder<SqlOperation> RenameKey(this MigrationBuilder migrationBuilder, string name, string schema, string newName)
        {
            name = name.Trim('[', ']');
            schema = schema.Trim('[', ']');
            newName = newName.Trim('[', ']');

            return migrationBuilder.Sql($"EXEC sp_rename '[{schema}].[{name}]', '{newName}';");
        }
    }
}
