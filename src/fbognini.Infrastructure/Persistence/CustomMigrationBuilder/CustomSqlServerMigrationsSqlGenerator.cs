using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using System.Linq;

namespace fbognini.Persistence.CustomMigrationBuilder
{
    /// <summary>
    /// An extended version of the default <see cref="SqlServerMigrationsSqlGenerator"/> 
    /// which adds functionality for creating and dropping User-Defined Table Types of SQL 
    /// server inside migration files using the same syntax as creating and dropping tables, 
    /// to use this generator, register it using <see cref="DbContextOptionsBuilder.ReplaceService{ISqlMigr, TImplementation}"/>
    /// in order to replace the default implementation of <see cref="IMigrationsSqlGenerator"/>
    /// </summary>
    public class CustomSqlServerMigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
    {
#if NET7_0_OR_GREATER

        public CustomSqlServerMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            ICommandBatchPreparer relationalAnnotations) : base(dependencies, relationalAnnotations)
        {
        }
#else

        public CustomSqlServerMigrationsSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            IRelationalAnnotationProvider relationalAnnotations) : base(dependencies, relationalAnnotations)
        {
        }
#endif

        protected override void Generate(
            MigrationOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            if (operation is CreateUserDefinedTableTypeOperation createUdtOperation)
            {
                GenerateCreateUdt(createUdtOperation, model, builder);
            }
            else if (operation is DropUserDefinedTableTypeOperation dropUdtOperation)
            {
                GenerateDropUdt(dropUdtOperation, builder);
            }
            else if (operation is CreateTableOperation createTableOperation)
            {
                base.Generate(createTableOperation, model, builder);
                GenerateCreateTriggerAfterCreateTable(createTableOperation, model, builder);
            }
            else
            {
                base.Generate(operation, model, builder);
            }
        }

        private void GenerateCreateTriggerAfterCreateTable(
            CreateTableOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            builder
                .Append("CREATE TRIGGER ")
                .AppendLine(Dependencies.SqlGenerationHelper.DelimitIdentifier($"{operation.Name}_Updated", operation.Schema))
                .Append("   ON ")
                .AppendLine(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine("AFTER INSERT, UPDATE")
                .AppendLine("AS")
                .AppendLine("BEGIN")
                .Append("   UPDATE ")
                .AppendLine(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine("   SET Updated = IIF(inserted.LastModified IS NULL OR inserted.Created > inserted.LastModified, IIF(inserted.LastModifiedNoAuth IS NULL OR inserted.Created > inserted.LastModifiedNoAuth, inserted.Created, inserted.LastModifiedNoAuth), IIF(inserted.LastModifiedNoAuth IS NULL OR inserted.LastModified > inserted.LastModifiedNoAuth, inserted.LastModified, inserted.LastModifiedNoAuth))")
                .AppendLine("   FROM inserted")
                .AppendLine("   WHERE 1 = 1");

            using (builder.Indent())
            {
                for (var i = 0; i < operation.PrimaryKey.Columns.Count(); i++)
                {
                    var column = operation.PrimaryKey.Columns[i];
                    builder.AppendLine($"AND {Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema)}.{column} = inserted.{column}");
                }

                builder.AppendLine();
            }

            builder
                .AppendLine("END")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }

        private void GenerateCreateUdt(
            CreateUserDefinedTableTypeOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            builder
                .Append("CREATE TYPE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(" AS TABLE (");

            using (builder.Indent())
            {
                for (var i = 0; i < operation.Columns.Count; i++)
                {
                    var column = operation.Columns[i];
                    ColumnDefinition(column, model, builder);

                    if (i != operation.Columns.Count - 1)
                    {
                        builder.AppendLine(",");
                    }
                }

                builder.AppendLine();
            }

            builder.Append(")");
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator).EndCommand();
        }

        private void GenerateDropUdt(
            DropUserDefinedTableTypeOperation operation,
            MigrationCommandListBuilder builder)
        {
            builder
                .Append("DROP TYPE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }

        //protected override void Generate(CreateTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        //{
        //    base.Generate(operation, model, builder);
        //    foreach (var columnOperation in operation.Columns) //columnOperation is AddColumnOperation
        //    {
        //        //operation.FindAnnotation("MyAttribute")
        //    }
        //}
    }

}
