using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobWize.Modules.Files.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveFileTablesToFilesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "files");

            migrationBuilder.RenameTable(
                name: "file_bindings",
                newName: "file_bindings",
                newSchema: "files");

            migrationBuilder.RenameTable(
                name: "file_assets",
                newName: "file_assets",
                newSchema: "files");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "file_bindings",
                schema: "files",
                newName: "file_bindings");

            migrationBuilder.RenameTable(
                name: "file_assets",
                schema: "files",
                newName: "file_assets");
        }
    }
}
