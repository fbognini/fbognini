using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OnUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                schema: "dbo",
                table: "Books",
                newName: "LastUpdatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "dbo",
                table: "Books",
                newName: "LastModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "Created",
                schema: "dbo",
                table: "Books",
                newName: "CreatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "LastUpdated",
                table: "Author",
                newName: "LastUpdatedOnUtc");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "Author",
                newName: "LastModifiedOnUtc");

            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Author",
                newName: "CreatedOnUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastUpdatedOnUtc",
                schema: "dbo",
                table: "Books",
                newName: "LastUpdated");

            migrationBuilder.RenameColumn(
                name: "LastModifiedOnUtc",
                schema: "dbo",
                table: "Books",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                schema: "dbo",
                table: "Books",
                newName: "Created");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedOnUtc",
                table: "Author",
                newName: "LastUpdated");

            migrationBuilder.RenameColumn(
                name: "LastModifiedOnUtc",
                table: "Author",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "CreatedOnUtc",
                table: "Author",
                newName: "Created");
        }
    }
}
