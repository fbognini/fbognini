using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fbognini.Infrastructure.Utilities
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
